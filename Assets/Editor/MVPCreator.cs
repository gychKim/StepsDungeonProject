using System.IO;
using UnityEditor;
using UnityEngine;

public class MVPCreatorWindow : EditorWindow
{
    string baseName = "Base";
    string targetFolder = "Assets/Scripts";

    [MenuItem("Window/Create MVP")]
    public static void ShowWindow()
    {
        var wnd = GetWindow<MVPCreatorWindow>();
        wnd.titleContent = new GUIContent("Create MVP");
        wnd.minSize = new Vector2(400, 140);
    }

    void OnGUI()
    {
        GUILayout.Label("MVP 스크립트 생성기", EditorStyles.boldLabel);
        baseName = EditorGUILayout.TextField("BaseName:", baseName);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Output Folder:", targetFolder);
        if (GUILayout.Button("Select...", GUILayout.MaxWidth(80)))
        {
            string selected = EditorUtility.OpenFolderPanel("Select Folder", Application.dataPath, "");
            if (!string.IsNullOrEmpty(selected) && selected.StartsWith(Application.dataPath))
            {
                targetFolder = "Assets" + selected.Substring(Application.dataPath.Length);
            }
            else
            {
                EditorUtility.DisplayDialog("Invalid Folder", "Unity 프로젝트 내부의 Assets 폴더 내에서 선택해야 합니다.", "OK");
            }
        }
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Generate Scripts"))
        {
            GenerateMVPScripts(baseName.Trim());
        }
    }

    void GenerateMVPScripts(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            EditorUtility.DisplayDialog("Error", "BaseName cannot be empty", "OK");
            return;
        }

        string folder = $"{targetFolder}/{name}";
        if (!AssetDatabase.IsValidFolder(folder))
        {
            // 폴더가 없으면 생성
            AssetDatabase.CreateFolder(targetFolder, name);
        }

        CreateFile(folder, $"{name}View.cs", GetViewTemplate(name));
        CreateFile(folder, $"{name}Model.cs", GetModelTemplate(name));
        CreateFile(folder, $"{name}Presenter.cs", GetPresenterTemplate(name));
		//CreateFile(folder, $"{name}UseCase.cs", GetUseCaseTemplate(name));

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Success", $"Created MVP scripts for '{name}' in selected folder.", "OK");
    }

    void CreateFile(string folder, string filename, string content)
    {
        string path = Path.Combine(folder, filename);
        if (!File.Exists(path))
            File.WriteAllText(path, content);
    }

    string GetViewTemplate(string n) =>
$@"using UnityEngine;
using UnityEngine.UI;

public class {n}View : BaseView
{{
    
}}";

    string GetModelTemplate(string n) =>
$@"using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using VContainer;

public class {n}Model : BaseModel
{{
    public override void Initialize()
    {{
        
    }}

    public override void Dispose()
    {{
        
    }}
}}";

    string GetPresenterTemplate(string n) =>
$@"using Cysharp.Threading.Tasks;
using R3;
using System;
using System.Threading;
using VContainer;

public class {n}Presenter : BasePresenter<{n}View, {n}Model>
{{
    public {n}Presenter({n}View view, {n}Model model) : base(view, model)
    {{
    }}

    public override void Initialize()
    {{
        
    }}

    public override void Dispose()
    {{
        Disposables.Dispose();
    }}
}}";

	string GetUseCaseTemplate(string n) =>
$@"public interface I{n}UseCase
{{
	
}}
public class {n}UseCase : I{n}UseCase
{{
	

	public {n}UseCase()
	{{
		
	}}

}}
";
}
