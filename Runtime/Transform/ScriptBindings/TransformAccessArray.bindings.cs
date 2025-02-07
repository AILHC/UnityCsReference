// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine.Bindings;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using System.Runtime.InteropServices;

namespace UnityEngine.Jobs
{
    //@TODO: Static code analysis needs to prevent creation of TransformAccess
    //       except through what is passed into the job function.
    //       Code below assumes this to be true since it doesn't check if the index is valid

    [StructLayout(LayoutKind.Sequential)]
    [NativeHeader("Runtime/Transform/ScriptBindings/TransformAccess.bindings.h")]
    public struct TransformAccess
    {
        private IntPtr hierarchy;
        private int index;
        private bool isReadOnly;

        public Vector3 position
        {
            get
            {
                CheckHierarchyValid();
                GetPosition(ref this, out var p);
                return p;
            }
            set
            {
                CheckHierarchyValid();
                CheckWriteAccess();
                SetPosition(ref this, ref value);
            }
        }


        public Quaternion rotation
        {
            get
            {
                CheckHierarchyValid();
                GetRotation(ref this, out var r);
                return r;
            }
            set
            {
                CheckHierarchyValid();
                CheckWriteAccess();
                SetRotation(ref this, ref value);
            }
        }

        public Vector3 localPosition
        {
            get
            {
                CheckHierarchyValid();
                GetLocalPosition(ref this, out var p);
                return p;
            }
            set
            {
                CheckHierarchyValid();
                CheckWriteAccess();
                SetLocalPosition(ref this, ref value);
            }
        }

        public Quaternion localRotation
        {
            get
            {
                CheckHierarchyValid();
                GetLocalRotation(ref this, out var r);
                return r;
            }
            set
            {
                CheckHierarchyValid();
                CheckWriteAccess();
                SetLocalRotation(ref this, ref value);
            }
        }

        public Vector3 localScale
        {
            get
            {
                CheckHierarchyValid();
                GetLocalScale(ref this, out var s);
                return s;
            }
            set
            {
                CheckHierarchyValid();
                CheckWriteAccess();
                SetLocalScale(ref this, ref value);
            }
        }


        public Matrix4x4 localToWorldMatrix
        {
            get
            {
                CheckHierarchyValid();
                GetLocalToWorldMatrix(ref this, out var m);
                return m;
            }
        }

        public Matrix4x4 worldToLocalMatrix
        {
            get
            {
                CheckHierarchyValid();
                GetWorldToLocalMatrix(ref this, out var m);
                return m;
            }
        }

        public bool isValid => hierarchy != IntPtr.Zero;

        //@TODO: Static code analysis needs to prevent creation of TransformAccess except through TransformAccessArray accessor.
        // Code below assumes this to be true since it doesn't check if TransformAccess is valid

        [NativeMethod(Name = "TransformAccessBindings::GetPosition", IsThreadSafe = true, IsFreeFunction = true, ThrowsException = true)]
        private static extern void GetPosition(ref TransformAccess access, out Vector3 p);

        [NativeMethod(Name = "TransformAccessBindings::SetPosition", IsThreadSafe = true, IsFreeFunction = true, ThrowsException = true)]
        private static extern void SetPosition(ref TransformAccess access, ref Vector3 p);

        [NativeMethod(Name = "TransformAccessBindings::GetRotation", IsThreadSafe = true, IsFreeFunction = true, ThrowsException = true)]
        private static extern void GetRotation(ref TransformAccess access, out Quaternion r);

        [NativeMethod(Name = "TransformAccessBindings::SetRotation", IsThreadSafe = true, IsFreeFunction = true, ThrowsException = true)]
        private static extern void SetRotation(ref TransformAccess access, ref Quaternion r);


        [NativeMethod(Name = "TransformAccessBindings::GetLocalPosition", IsThreadSafe = true, IsFreeFunction = true, ThrowsException = true)]
        private static extern void GetLocalPosition(ref TransformAccess access, out Vector3 p);

        [NativeMethod(Name = "TransformAccessBindings::SetLocalPosition", IsThreadSafe = true, IsFreeFunction = true, ThrowsException = true)]
        private static extern void SetLocalPosition(ref TransformAccess access, ref Vector3 p);

        [NativeMethod(Name = "TransformAccessBindings::GetLocalRotation", IsThreadSafe = true, IsFreeFunction = true, ThrowsException = true)]
        private static extern void GetLocalRotation(ref TransformAccess access, out Quaternion r);

        [NativeMethod(Name = "TransformAccessBindings::SetLocalRotation", IsThreadSafe = true, IsFreeFunction = true, ThrowsException = true)]
        private static extern void SetLocalRotation(ref TransformAccess access, ref Quaternion r);


        [NativeMethod(Name = "TransformAccessBindings::GetLocalScale", IsThreadSafe = true, IsFreeFunction = true, ThrowsException = true)]
        private static extern void GetLocalScale(ref TransformAccess access, out Vector3 r);

        [NativeMethod(Name = "TransformAccessBindings::SetLocalScale", IsThreadSafe = true, IsFreeFunction = true, ThrowsException = true)]
        private static extern void SetLocalScale(ref TransformAccess access, ref Vector3 r);

        [NativeMethod(Name = "TransformAccessBindings::GetLocalToWorldMatrix", IsThreadSafe = true, IsFreeFunction = true, ThrowsException = true)]
        private static extern void GetLocalToWorldMatrix(ref TransformAccess access, out Matrix4x4 m);

        [NativeMethod(Name = "TransformAccessBindings::GetWorldToLocalMatrix", IsThreadSafe = true, IsFreeFunction = true, ThrowsException = true)]
        private static extern void GetWorldToLocalMatrix(ref TransformAccess access, out Matrix4x4 m);

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
        internal void CheckHierarchyValid()
        {
            if (!isValid)
                throw new NullReferenceException("The TransformAccess is not valid and points to an invalid hierarchy");
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
        internal void MarkReadWrite()
        {
            isReadOnly = false;
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
        internal void MarkReadOnly()
        {
            isReadOnly = true;
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
        void CheckWriteAccess()
        {
            if (isReadOnly)
                throw new InvalidOperationException("Cannot write to TransformAccess since the transform job was scheduled as read-only");
        }

        //@TODO: API incomplete...
    }

    [StructLayout(LayoutKind.Sequential)]
    [NativeType(Header = "Runtime/Transform/ScriptBindings/TransformAccess.bindings.h", CodegenOptions = CodegenOptions.Custom)]
    public struct TransformAccessArray : IDisposable
    {
        IntPtr              m_TransformArray;

        AtomicSafetyHandle  m_Safety;
        DisposeSentinel     m_DisposeSentinel;

        public TransformAccessArray(Transform[] transforms, int desiredJobCount = -1)
        {
            Allocate(transforms.Length, desiredJobCount, out this);
            SetTransforms(m_TransformArray, transforms);
        }

        public TransformAccessArray(int capacity, int desiredJobCount = -1)
        {
            Allocate(capacity, desiredJobCount, out this);
        }

        public static void Allocate(int capacity, int desiredJobCount, out TransformAccessArray array)
        {
            array.m_TransformArray = Create(capacity, desiredJobCount);

            DisposeSentinel.Create(out array.m_Safety, out array.m_DisposeSentinel, 1, Allocator.Persistent);
        }

        public bool isCreated
        {
            get { return m_TransformArray != IntPtr.Zero; }
        }

        public void Dispose()
        {
            DisposeSentinel.Dispose(ref m_Safety, ref m_DisposeSentinel);

            DestroyTransformAccessArray(m_TransformArray);
            m_TransformArray = IntPtr.Zero;
        }

        internal IntPtr GetTransformAccessArrayForSchedule()
        {
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);

            return m_TransformArray;
        }

        public Transform this[int index]
        {
            get
            {
                AtomicSafetyHandle.CheckReadAndThrow(m_Safety);

                return GetTransform(m_TransformArray, index);
            }
            set
            {
                AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);

                SetTransform(m_TransformArray, index, value);
            }
        }

        public int capacity
        {
            get
            {
                AtomicSafetyHandle.CheckReadAndThrow(m_Safety);

                return GetCapacity(m_TransformArray);
            }
            set
            {
                AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);

                SetCapacity(m_TransformArray, value);
            }
        }

        public int length
        {
            get
            {
                AtomicSafetyHandle.CheckReadAndThrow(m_Safety);

                return GetLength(m_TransformArray);
            }
        }

        public void Add(Transform transform)
        {
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);

            Add(m_TransformArray, transform);
        }

        public void RemoveAtSwapBack(int index)
        {
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);

            RemoveAtSwapBack(m_TransformArray, index);
        }

        public void SetTransforms(Transform[] transforms)
        {
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);

            SetTransforms(m_TransformArray, transforms);
        }

        [NativeMethod(Name = "TransformAccessArrayBindings::Create", IsFreeFunction = true)]
        private static extern IntPtr Create(int capacity, int desiredJobCount);

        [NativeMethod(Name = "DestroyTransformAccessArray", IsFreeFunction = true)]
        private static extern void DestroyTransformAccessArray(IntPtr transformArray);

        [NativeMethod(Name = "TransformAccessArrayBindings::SetTransforms", IsFreeFunction = true)]
        private static extern void SetTransforms(IntPtr transformArrayIntPtr, Transform[] transforms);

        [NativeMethod(Name = "TransformAccessArrayBindings::AddTransform", IsFreeFunction = true)]
        private static extern void Add(IntPtr transformArrayIntPtr, Transform transform);

        [NativeMethod(Name = "TransformAccessArrayBindings::RemoveAtSwapBack", IsFreeFunction = true, ThrowsException = true)]
        private static extern void RemoveAtSwapBack(IntPtr transformArrayIntPtr, int index);

        [NativeMethod(Name = "TransformAccessArrayBindings::GetSortedTransformAccess", IsThreadSafe = true, IsFreeFunction = true, ThrowsException = true)]
        internal static extern IntPtr GetSortedTransformAccess(IntPtr transformArrayIntPtr);

        [NativeMethod(Name = "TransformAccessArrayBindings::GetSortedToUserIndex", IsThreadSafe = true, IsFreeFunction = true, ThrowsException = true)]
        internal static extern IntPtr GetSortedToUserIndex(IntPtr transformArrayIntPtr);

        [NativeMethod(Name = "TransformAccessArrayBindings::GetLength", IsFreeFunction = true)]
        internal static extern int GetLength(IntPtr transformArrayIntPtr);

        [NativeMethod(Name = "TransformAccessArrayBindings::GetCapacity", IsFreeFunction = true)]
        internal static extern int GetCapacity(IntPtr transformArrayIntPtr);

        [NativeMethod(Name = "TransformAccessArrayBindings::SetCapacity", IsFreeFunction = true)]
        internal static extern void SetCapacity(IntPtr transformArrayIntPtr, int capacity);

        [NativeMethod(Name = "TransformAccessArrayBindings::GetTransform", IsFreeFunction = true, ThrowsException = true)]
        internal static extern Transform GetTransform(IntPtr transformArrayIntPtr, int index);

        [NativeMethod(Name = "TransformAccessArrayBindings::SetTransform", IsFreeFunction = true, ThrowsException = true)]
        internal static extern void SetTransform(IntPtr transformArrayIntPtr, int index, Transform transform);
    }
}
