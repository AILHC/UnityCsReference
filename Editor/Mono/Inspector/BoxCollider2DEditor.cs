// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

using System;
using UnityEditor.IMGUI.Controls;
using UnityEditor.AnimatedValues;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEditorInternal;

namespace UnityEditor
{
    [CustomEditor(typeof(BoxCollider2D))]
    [CanEditMultipleObjects]
    class BoxCollider2DEditor : Collider2DEditorBase
    {
        SerializedProperty m_Size;
        SerializedProperty m_EdgeRadius;
        SerializedProperty m_UsedByComposite;
        readonly AnimBool m_ShowCompositeRedundants = new AnimBool();

        public override void OnEnable()
        {
            base.OnEnable();

            m_Size = serializedObject.FindProperty("m_Size");
            m_EdgeRadius = serializedObject.FindProperty("m_EdgeRadius");
            m_UsedByComposite = serializedObject.FindProperty("m_UsedByComposite");
            m_AutoTiling = serializedObject.FindProperty("m_AutoTiling");
            m_ShowCompositeRedundants.value = !m_UsedByComposite.boolValue;
            m_ShowCompositeRedundants.valueChanged.AddListener(Repaint);
        }

        public override void OnDisable()
        {
            base.OnDisable();

            m_ShowCompositeRedundants.valueChanged.RemoveListener(Repaint);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            bool disableEditCollider = !CanEditCollider();

            if (disableEditCollider)
            {
                EditorGUILayout.HelpBox(Styles.s_ColliderEditDisableHelp.text, MessageType.Info);

                if (ToolManager.activeToolType == typeof(BoxCollider2DTool))
                    ToolManager.RestorePreviousTool();
            }
            else
                EditorGUILayout.EditorToolbarForTarget(EditorGUIUtility.TrTempContent("Edit Collider"), this);

            GUILayout.Space(5);
            base.OnInspectorGUI();

            EditorGUILayout.PropertyField(m_Size);

            m_ShowCompositeRedundants.target = !m_UsedByComposite.boolValue;
            if (EditorGUILayout.BeginFadeGroup(m_ShowCompositeRedundants.faded))
                EditorGUILayout.PropertyField(m_EdgeRadius);
            EditorGUILayout.EndFadeGroup();

            serializedObject.ApplyModifiedProperties();

            FinalizeInspectorGUI();
        }
    }

    [EditorTool("Edit Box Collider 2D", typeof(BoxCollider2D))]
    class BoxCollider2DTool : EditorTool
    {
        public override GUIContent toolbarIcon
        {
            get { return PrimitiveBoundsHandle.editModeButton; }
        }

        readonly BoxBoundsHandle m_BoundsHandle = new BoxBoundsHandle();

        void OnEnable()
        {
            m_BoundsHandle.axes = BoxBoundsHandle.Axes.X | BoxBoundsHandle.Axes.Y;
        }

        public override void OnToolGUI(EditorWindow window)
        {
            foreach (var obj in targets)
            {
                BoxCollider2D collider = obj as BoxCollider2D;

                if (collider == null || Mathf.Approximately(collider.transform.lossyScale.sqrMagnitude, 0f))
                    continue;

                // collider matrix is 2d projection of transform's rotation onto x/y plane about transform's origin
                Matrix4x4 handleMatrix = collider.transform.localToWorldMatrix;
                handleMatrix.SetRow(0, Vector4.Scale(handleMatrix.GetRow(0), new Vector4(1f, 1f, 0f, 1f)));
                handleMatrix.SetRow(1, Vector4.Scale(handleMatrix.GetRow(1), new Vector4(1f, 1f, 0f, 1f)));
                handleMatrix.SetRow(2, new Vector4(0f, 0f, 1f, collider.transform.position.z));

                if (collider.usedByComposite && collider.composite != null)
                {
                    // composite offset is rotated by composite's transformation matrix and projected back onto 2D plane
                    var compositeOffset = collider.composite.transform.rotation * collider.composite.offset;
                    compositeOffset.z = 0f;
                    handleMatrix = Matrix4x4.TRS(compositeOffset, Quaternion.identity, Vector3.one) * handleMatrix;
                }

                using (new Handles.DrawingScope(handleMatrix))
                {
                    m_BoundsHandle.center = collider.offset;
                    m_BoundsHandle.size = collider.size;
                    m_BoundsHandle.SetColor(collider.enabled ? Handles.s_ColliderHandleColor : Handles.s_ColliderHandleColorDisabled);
                    EditorGUI.BeginChangeCheck();
                    m_BoundsHandle.DrawHandle();
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(collider, string.Format("Modify {0}", ObjectNames.NicifyVariableName(collider.GetType().Name)));

                        // test for size change after using property setter in case input data was sanitized
                        Vector2 oldSize = collider.size;
                        collider.size = m_BoundsHandle.size;

                        // because projection of offset is a lossy operation, only do it if the size has actually changed
                        // this check prevents drifting while dragging handle when size is zero (case 863949)
                        if (collider.size != oldSize)
                            collider.offset = m_BoundsHandle.center;
                    }
                }
            }
        }
    }
}
