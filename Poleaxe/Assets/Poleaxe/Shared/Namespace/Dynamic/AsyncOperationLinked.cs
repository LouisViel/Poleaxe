using System;
using UnityEngine;

namespace Poleaxe
{
    public class AsyncOperationLinked
    {
        public AsyncOperationLinked() { }
        public AsyncOperationLinked(AsyncOperation operation) => LinkOperation(operation);
        public AsyncOperationLinked(AsyncOperationLinked operation) => LinkOperation(operation);

        private AsyncOperation asyncOperation;
        private AsyncOperationLinked linkedOperation;

        public void LinkOperation(AsyncOperation operation)
        {
            if (linkedOperation != null || asyncOperation != null || operation == null) return;
            asyncOperation = operation;
            asyncOperation.completed += CompleteCallback;
            allowSceneActivation = allowSceneActivation;
            priority = priority;
        }

        public void LinkOperation(AsyncOperationLinked operation)
        {
            if (linkedOperation != null || asyncOperation != null || operation == null) return;
            linkedOperation = operation;
            linkedOperation.completed += CompleteCallback;
            allowSceneActivation = allowSceneActivation;
            priority = priority;
        }

        private AsyncOperation GetOperation()
        {
            if (asyncOperation != null) return asyncOperation;
            if (linkedOperation == null) return null;
            return linkedOperation.GetOperation();
        }

        private void CompleteCallback(AsyncOperation _)
        {
            if (m_completeCallback != null) {
                m_completeCallback(GetOperation());
                m_completeCallback = null;
            }
        }

        private Action<AsyncOperation> m_completeCallback;
        public event Action<AsyncOperation> completed {
            add {
                if (isDone) value(GetOperation());
                else m_completeCallback = (Action<AsyncOperation>)Delegate.Combine(m_completeCallback, value); }
            remove { m_completeCallback = (Action<AsyncOperation>)Delegate.Remove(m_completeCallback, value); }
        }

        private int m_priority = 0;
        public int priority {
            get => m_priority;
            set {
                m_priority = value;
                if (asyncOperation != null) asyncOperation.priority = m_priority;
                else if (linkedOperation != null) linkedOperation.priority = m_priority;
            }
        }

        private bool m_allowSceneActivation = true;
        public bool allowSceneActivation {
            get => m_allowSceneActivation;
            set {
                m_allowSceneActivation = value;
                if (asyncOperation != null) asyncOperation.allowSceneActivation = m_allowSceneActivation;
                else if (linkedOperation != null) linkedOperation.allowSceneActivation = m_allowSceneActivation;
            }
        }

        private float m_progress = 0f;
        public float progress {
            get {
                if (asyncOperation != null) return asyncOperation.progress;
                if (linkedOperation != null) return linkedOperation.progress;
                return m_progress;
            }
        }

        private bool m_isDone = false;
        public bool isDone {
            get {
                if (asyncOperation != null) return asyncOperation.isDone;
                if (linkedOperation != null) return linkedOperation.isDone;
                return m_isDone;
            }
        }
    }
}