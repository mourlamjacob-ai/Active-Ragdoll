using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace ActiveRagdoll.Editor
{
    public class ActiveRagdollWizardWindow : EditorWindow
    {
        private ActiveRagdollWizardSettings _settings;
        private SerializedObject _serializedSettings;

        [MenuItem("Tools/Active Ragdoll/Wizard")]
        public static void OpenWindow()
        {
            GetWindow<ActiveRagdollWizardWindow>("Active Ragdoll Wizard");
        }

        public static void OpenWindow(ActiveRagdollWizardSettings settings)
        {
            ActiveRagdollWizardWindow window = GetWindow<ActiveRagdollWizardWindow>("Active Ragdoll Wizard");
            window.SetSettings(settings);
        }

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceId, int line)
        {
            Object asset = EditorUtility.InstanceIDToObject(instanceId);
            if (asset is ActiveRagdollWizardSettings settings)
            {
                OpenWindow(settings);
                return true;
            }

            return false;
        }

        private void OnEnable()
        {
            if (_settings == null)
            {
                string[] guids = AssetDatabase.FindAssets("t:ActiveRagdollWizardSettings");
                if (guids.Length > 0)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    SetSettings(AssetDatabase.LoadAssetAtPath<ActiveRagdollWizardSettings>(path));
                }
            }
        }

        private void SetSettings(ActiveRagdollWizardSettings settings)
        {
            _settings = settings;
            _serializedSettings = _settings != null ? new SerializedObject(_settings) : null;
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Active Ragdoll Setup Wizard", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            using (new EditorGUILayout.HorizontalScope())
            {
                _settings = (ActiveRagdollWizardSettings)EditorGUILayout.ObjectField("Settings Asset", _settings, typeof(ActiveRagdollWizardSettings), false);

                if (GUILayout.Button("New", GUILayout.Width(80f)))
                {
                    CreateSettingsAsset();
                }
            }

            if (_settings == null)
            {
                EditorGUILayout.HelpBox("Create or assign a settings asset to continue.", MessageType.Info);
                return;
            }

            if (_serializedSettings == null || _serializedSettings.targetObject != _settings)
            {
                _serializedSettings = new SerializedObject(_settings);
            }

            _serializedSettings.Update();
            SerializedProperty iterator = _serializedSettings.GetIterator();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                if (iterator.name == "m_Script")
                {
                    using (new EditorGUI.DisabledScope(true))
                    {
                        EditorGUILayout.PropertyField(iterator, true);
                    }
                }
                else
                {
                    EditorGUILayout.PropertyField(iterator, true);
                }

                enterChildren = false;
            }
            _serializedSettings.ApplyModifiedProperties();

            EditorGUILayout.Space();
            if (GUILayout.Button("Create Active Ragdoll", GUILayout.Height(36f)))
            {
                ActiveRagdollWizardBuilder.BuildFromSettings(_settings);
            }
        }

        private void CreateSettingsAsset()
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Create Active Ragdoll Settings",
                "ActiveRagdollWizardSettings",
                "asset",
                "Choose where to save the wizard settings asset.");

            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            ActiveRagdollWizardSettings asset = CreateInstance<ActiveRagdollWizardSettings>();
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            SetSettings(asset);
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }
    }
}
