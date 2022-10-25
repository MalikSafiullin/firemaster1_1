using System;
using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif

#pragma warning disable 618, 649
namespace UnityStandardAssets.Utility
{
#if UNITY_EDITOR

    [ExecuteInEditMode]
#endif
    public class PlatformSpecificContent : MonoBehaviour
#if UNITY_EDITOR
        , UnityEditor.Build.IActiveBuildTargetChanged
#endif
    {
        private enum BuildTargetGroup
        {
            Standalone,
            Mobile
        }

        [FormerlySerializedAs("m_BuildTargetGroup")] [SerializeField]
        private BuildTargetGroup mBuildTargetGroup;
        [FormerlySerializedAs("m_Content")] [SerializeField]
        private GameObject[] mContent = new GameObject[0];
        [FormerlySerializedAs("m_MonoBehaviours")] [SerializeField]
        private MonoBehaviour[] mMonoBehaviours = new MonoBehaviour[0];
        [FormerlySerializedAs("m_ChildrenOfThisObject")] [SerializeField]
        private bool mChildrenOfThisObject;

#if !UNITY_EDITOR
	void OnEnable()
	{
		CheckEnableContent();
	}
#else
        public int callbackOrder
        {
            get
            {
                return 1;
            }
        }
#endif

#if UNITY_EDITOR

        private void OnEnable()
        {
            EditorApplication.update += Update;
        }


        private void OnDisable()
        {
            EditorApplication.update -= Update;
        }

        public void OnActiveBuildTargetChanged(BuildTarget previousTarget, BuildTarget newTarget)
        {
            CheckEnableContent();
        }

        private void Update()
        {
            CheckEnableContent();
        }
#endif


        private void CheckEnableContent()
        {
#if (UNITY_IPHONE || UNITY_ANDROID || UNITY_WP8 || UNITY_TIZEN)
		if (m_BuildTargetGroup == BuildTargetGroup.Mobile)
		{
			EnableContent(true);
		} else {
			EnableContent(false);
		}
#endif

#if !(UNITY_IPHONE || UNITY_ANDROID || UNITY_WP8 || UNITY_TIZEN)
            if (mBuildTargetGroup == BuildTargetGroup.Mobile)
            {
                EnableContent(false);
            }
            else
            {
                EnableContent(true);
            }
#endif
        }


        private void EnableContent(bool enabled)
        {
            if (mContent.Length > 0)
            {
                foreach (var g in mContent)
                {
                    if (g != null)
                    {
                        g.SetActive(enabled);
                    }
                }
            }
            if (mChildrenOfThisObject)
            {
                foreach (Transform t in transform)
                {
                    t.gameObject.SetActive(enabled);
                }
            }
            if (mMonoBehaviours.Length > 0)
            {
                foreach (var monoBehaviour in mMonoBehaviours)
                {
                    monoBehaviour.enabled = enabled;
                }
            }
        }
    }
}