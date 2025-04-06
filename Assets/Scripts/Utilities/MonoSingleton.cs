using System;
using UnityEngine;

namespace Utilities
{
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
		private static T _instance;

		public static T Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new GameObject().AddComponent<T>();
					_instance.Init();
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
