using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace OUI.Editor
{
    /// <summary>
    /// OUI代码生成器
    /// </summary>
    public static class OUICodeGenerator
    {
        [MenuItem("GameObject/生成OUI代码", false, 0)]
        private static void GenerateCode()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null)
            {
                Debug.LogWarning("请先在Hierarchy中选择一个GameObject");
                return;
            }

            string code = Generate(selected.transform);
            GUIUtility.systemCopyBuffer = code;
            Debug.Log("代码已生成并复制到剪贴板");
        }

        [MenuItem("GameObject/生成OUI代码", true)]
        private static bool ValidateGenerateCode()
        {
            return Selection.activeGameObject != null;
        }

        private static string Generate(Transform root)
        {
            List<BindInfo> bindings = new List<BindInfo>();
            CollectBindings(root, root, bindings);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("#region 脚本工具生成的代码");
            sb.AppendLine();

            // 生成字段声明
            foreach (var binding in bindings)
            {
                sb.AppendLine($"private {binding.TypeName} {binding.FieldName};");
            }

            sb.AppendLine();
            sb.AppendLine("public override void BindUI()");
            sb.AppendLine("{");

            // 生成绑定代码
            foreach (var binding in bindings)
            {
                if (binding.TypeName == "GameObject")
                {
                    sb.AppendLine($"    {binding.FieldName} = FindChild(\"{binding.Path}\").gameObject;");
                }
                else if (binding.TypeName == "Transform")
                {
                    sb.AppendLine($"    {binding.FieldName} = FindChild(\"{binding.Path}\");");
                }
                else
                {
                    sb.AppendLine($"    {binding.FieldName} = FindChildComponent<{binding.TypeName}>(\"{binding.Path}\");");
                }
            }

            // 生成Button事件绑定
            foreach (var binding in bindings)
            {
                if (binding.TypeName == "Button")
                {
                    string callbackName = GetButtonCallbackName(binding.FieldName);
                    sb.AppendLine($"    {binding.FieldName}.onClick.AddListener({callbackName});");
                }
            }

            sb.AppendLine("}");
            sb.AppendLine();
            sb.AppendLine("#endregion");

            return sb.ToString();
        }

        private static void CollectBindings(Transform root, Transform current, List<BindInfo> bindings)
        {
            for (int i = 0; i < current.childCount; i++)
            {
                Transform child = current.GetChild(i);
                string name = child.name;

                if (name.StartsWith("m_"))
                {
                    string path = GetRelativePath(root, child);
                    BindInfo info = new BindInfo
                    {
                        FieldName = name,
                        Path = path,
                        TypeName = GetTypeName(name, child)
                    };
                    bindings.Add(info);

                    // m_item_ 前缀的节点不递归扫描子节点
                    if (name.StartsWith("m_item_"))
                    {
                        continue;
                    }
                }

                // 递归扫描子节点
                CollectBindings(root, child, bindings);
            }
        }

        private static string GetRelativePath(Transform root, Transform target)
        {
            if (target == root)
                return "";

            List<string> path = new List<string>();
            Transform current = target;

            while (current != root && current != null)
            {
                path.Insert(0, current.name);
                current = current.parent;
            }

            return string.Join("/", path);
        }

        private static string GetTypeName(string name, Transform transform)
        {
            if (name.StartsWith("m_btn_"))
            {
                return "Button";
            }
            else if (name.StartsWith("m_img_"))
            {
                return "Image";
            }
            else if (name.StartsWith("m_tmp_"))
            {
                return "TextMeshProUGUI";
            }
            else if (name.StartsWith("m_tf_"))
            {
                return "Transform";
            }
            else if (name.StartsWith("m_go_") || name.StartsWith("m_item_"))
            {
                return "GameObject";
            }

            return "Transform";
        }

        private static string GetButtonCallbackName(string fieldName)
        {
            // m_btn_continue -> OnClick_continueBtn
            if (fieldName.StartsWith("m_btn_"))
            {
                string suffix = fieldName.Substring(6); // 去掉 "m_btn_"
                return $"OnClick_{suffix}Btn";
            }
            return "OnClick";
        }

        private class BindInfo
        {
            public string FieldName;
            public string Path;
            public string TypeName;
        }
    }
}
