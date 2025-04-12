using System;
using Unity.VisualScripting;
using UnityEngine;

namespace Utilities
{
    public abstract class PrefabSingleton<T> : MonoBehaviour where T : PrefabSingleton<T>
    {
		private static string PrefabPath = string.Empty;

		private static T _instance;

		public static T Instance
		{
			get
			{
				if (_instance == null)
				{
					var result = FindFirstObjectByType<T>();
					if(result != null)
					{
						_instance = result;
					}
					else
					{
						_instance = Instantiate(Resources.Load(PrefabPath)).GetComponent<T>();
					}
				}

				return _instance;
			}
		}

        public static bool HasInstanceCreated => _instance != null;

		private bool _isInitialised;

        private void Awake()
        {
            if (_isInitialised)
			{
				return;
			}

			if (HasInstanceCreated)
			{
				throw new InvalidOperationException("Multiple instances of a singleton have been instantiated. This is not allowed.");
			}

			Init();
        }

        public void Init()
        {
			DontDestroyOnLoad(gameObject);
			OnInit();
			_instance = (T)this;
			_isInitialised = true;
        }

        protected abstract void OnInit();

    }
}
