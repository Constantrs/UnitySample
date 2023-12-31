using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using static UnityEditor.Searcher.Searcher.AnalyticsEvent;
using TaskSample.Command;

namespace TaskSample
{
    public enum EventType
    {
        Input,
        PauseStart,
        PauseEnd,
        End
    }

    public class MainSystem : MonoBehaviour
    {
        public enum State
        {
            Uninitialized,
            PushCommand,
            ExecuteCommand,
            Wait,
            Exit
        }

        private static MainSystem instance;

        public FramerateMode mode;
        public State state = State.Uninitialized;

        [TextArea]
        public string text;

        public GameObject worldRoot;
        public TMP_Text UIText;

        private List<IObserver> _Observers = new List<IObserver>();
        private WorldManager _WorldManager = new WorldManager();
        private TextManager _TextManager = new TextManager();

        private CancellationToken _cts;
        private bool _Pause = false;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                FrameManager.SetFramerateMode(mode);

                _WorldManager.Initialize(this);
                _TextManager.Initialize(this);

                _cts = this.GetCancellationTokenOnDestroy();
                state = State.PushCommand;

                OnLastEarlyUpdate().Forget();
                OnPostLateUpdate().Forget();
            }
            else
            {

            }
        }
        private void OnDestroy()
        {
            NotifyOvservers(EventType.End);  
            _Observers.Clear();

            if (instance == this)
            {
                instance = null;
            }
        }

        public static MainSystem GetInstance()
        {
            return instance;
        }

        public WorldManager GetWorldManager()
        {
            return _WorldManager;
        }

        public TextManager GetTextManager()
        {
            return _TextManager;
        }

        public float GetTimeMultiplier()
        {
            if (_Pause)
            {
                return 0.0f;
            }
            else
            {
                return FrameManager.GetTimeMultiplier(mode);
            }
        }

        public void Pause()
        {
            SetPause(!_Pause);
        }

        public async UniTask OnLastEarlyUpdate()
        {
            while (state != State.Exit)
            {
                await UniTask.Yield(PlayerLoopTiming.LastEarlyUpdate, _cts);
            }
        }

        public async UniTask OnPostLateUpdate()
        {
            while (state != State.Exit)
            {
                if(state == State.ExecuteCommand) 
                {
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        NotifyOvservers(EventType.Input);
                    }

                    WaitCommand();
                }

                await UniTask.Yield(PlayerLoopTiming.PostLateUpdate, _cts);
            }
        }

        public void PlayText()
        {
            if (state != State.PushCommand)
            {
                return;
            }

            var commandManager = CommandManager.GetInstance();
            if (commandManager != null)
            {
                object[] str = { (object)text };
                commandManager.PushProcess("setmessage", str);
                commandManager.Execute().Forget();
                state = State.ExecuteCommand;
            }
        }

        public void SetPause(bool pause)
        {
            _Pause = pause;
            if (pause)
            {
                NotifyOvservers(EventType.PauseStart);
            }
            else
            {
                NotifyOvservers(EventType.PauseEnd);
            }
        }

        public void RandomWait()
        {
            if (state != State.PushCommand)
            {
                return;
            }

            var commandManager = CommandManager.GetInstance();
            if (commandManager != null)
            {
                commandManager.PushProcess("waitsecond", null);
                commandManager.Execute().Forget();
                state = State.ExecuteCommand;
            }
        }

        public void WaitCommand()
        {
            var commandManager = CommandManager.GetInstance();
            if (commandManager == null)
            {
                return;    
            }

            if(!commandManager.IsExecuting())
            {
                state = State.Wait;
            }
        }

        public void AddObserver(IObserver observer)
        {
            _Observers.Add(observer);
        }

        public void RemoveOvserver(IObserver observer)
        {
            _Observers.Remove(observer);
        }

        protected void NotifyOvservers(EventType eventType)
        {
            _Observers.ForEach(observer => observer.OnNotify(eventType));
        }
    }
}
