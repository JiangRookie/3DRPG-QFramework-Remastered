using System;
using System.Collections.Generic;
using UnityEngine;

namespace QFramework
{
    #region Architecture

    public interface IArchitecture
    {
        void RegisterSystem<T>(T system) where T : ISystem;

        void RegisterModel<T>(T model) where T : IModel;

        void RegisterUtility<T>(T utility) where T : IUtility;

        T GetSystem<T>() where T : class, ISystem;

        T GetModel<T>() where T : class, IModel;

        T GetUtility<T>() where T : class, IUtility;

        void SendCommand<T>() where T : ICommand, new();

        void SendCommand<T>(T command) where T : ICommand;

        TResult SendQuery<TResult>(IQuery<TResult> query);

        IUnRegister RegisterEvent<T>(Action<T> onEvent);

        void SendEvent<T>() where T : new();

        void SendEvent<T>(T e);

        void UnRegisterEvent<T>(Action<T> onEvent);
    }

    public abstract class Architecture<T> : IArchitecture where T : Architecture<T>, new()
    {
        /// <summary> 类似单例，仅在内部访问 </summary>
        static T s_Architecture = null;

        public static Action<T> OnRegisterPatch = architecture => { };

        IOCContainer m_Container = new IOCContainer();

        /// <summary> Architecture 是否初始化完成 </summary>
        bool m_Inited = false;

        /// <summary> 用于缓存需要初始化的 Model 集合 </summary>
        List<IModel> m_Models = new List<IModel>();

        /// <summary> 用于缓存需要初始化的 System 集合 </summary>
        List<ISystem> m_Systems = new List<ISystem>();

        ///<summary> 确保 Architecture 是有实例的 </summary>> 
        static void MakeSureArchitecture()
        {
            if (s_Architecture == null)
            {
                s_Architecture = new T();
                s_Architecture.Init();

                OnRegisterPatch?.Invoke(s_Architecture);

                /*
                 * 由于 Model 层是比 System 层更底层的，所有 System 层是可以直接访问 Model 层的，
                 * 所以这里需要确保 Model 的初始化是在 System 初始化之前进行。
                 */

                // 初始化 Model
                foreach (var architectureModel in s_Architecture.m_Models)
                {
                    architectureModel.Init();
                }

                s_Architecture.m_Models.Clear();

                foreach (var architectureSystem in s_Architecture.m_Systems)
                {
                    architectureSystem.Init();
                }

                s_Architecture.m_Systems.Clear();

                // 清空 Model
                s_Architecture.m_Inited = true;
            }
        }

        /// <summary> 返回 Architecture 实例 </summary>
        public static IArchitecture Interface
        {
            get
            {
                if (s_Architecture == null)
                {
                    MakeSureArchitecture();
                }
                return s_Architecture;
            }
        }

        public void RegisterSystem<TSystem>(TSystem system) where TSystem : ISystem
        {
            system.SetArchitecture(this);
            m_Container.Register<TSystem>(system);

            if (m_Inited) // 如果 Architecture 已经初始化了，则让 Model 自己初始化
            {
                system.Init();
            }
            else // 如果 Architecture 没有初始化，则将 Model 添加到 m_Models 列表中让 Architecture 统一初始化
            {
                m_Systems.Add(system);
            }
        }

        public void RegisterModel<TModel>(TModel model) where TModel : IModel
        {
            model.SetArchitecture(this);
            m_Container.Register<TModel>(model);

            if (m_Inited) // 如果 Architecture 已经初始化了，则让 Model 自己初始化
            {
                model.Init();
            }
            else // 如果 Architecture 没有初始化，则将 Model 添加到 m_Models 列表中让 Architecture 统一初始化
            {
                m_Models.Add(model);
            }
        }

        public void RegisterUtility<TUtility>(TUtility utility) where TUtility : IUtility
        {
            m_Container.Register<TUtility>(utility);
        }

        public TSystem GetSystem<TSystem>() where TSystem : class, ISystem
        {
            return m_Container.Get<TSystem>();
        }

        public TModel GetModel<TModel>() where TModel : class, IModel
        {
            return m_Container.Get<TModel>();
        }

        public TUtility GetUtility<TUtility>() where TUtility : class, IUtility
        {
            return m_Container.Get<TUtility>();
        }

        public void SendCommand<TCommand>() where TCommand : ICommand, new()
        {
            var command = new TCommand();
            command.SetArchitecture(this);
            command.Execute();
        }

        public void SendCommand<TCommand>(TCommand command) where TCommand : ICommand
        {
            command.SetArchitecture(this);
            command.Execute();
        }

        public TResult SendQuery<TResult>(IQuery<TResult> query)
        {
            query.SetArchitecture(this);
            return query.Do();
        }

        // 留给子类注册模块
        protected abstract void Init();

        #region Event

        ITypeEventSystem m_TypeEventSystem = new TypeEventSystem();

        public IUnRegister RegisterEvent<TEvent>(Action<TEvent> onEvent)
        {
            return m_TypeEventSystem.Register<TEvent>(onEvent);
        }

        public void SendEvent<TEvent>() where TEvent : new()
        {
            m_TypeEventSystem.Send<TEvent>();
        }

        public void SendEvent<TEvent>(TEvent e)
        {
            m_TypeEventSystem.Send<TEvent>(e);
        }

        public void UnRegisterEvent<TEvent>(Action<TEvent> onEvent)
        {
            m_TypeEventSystem.UnRegister<TEvent>(onEvent);
        }

        #endregion
    }

    #endregion

    #region Controller 表现层

    public interface IController : IBelongToArchitecture, ICanGetSystem, ICanGetModel, ICanSendCommand
                                 , ICanSendQuery, ICanRegisterEvent { }

    #endregion

    #region System 系统层

    public interface ISystem : IBelongToArchitecture, ICanSetArchitecture, ICanGetSystem, ICanGetModel, ICanGetUtility
                             , ICanRegisterEvent, ICanSendEvent
    {
        void Init();
    }

    public abstract class AbstractSystem : ISystem
    {
        IArchitecture m_Architecture;
        IArchitecture IBelongToArchitecture.GetArchitecture() => m_Architecture;
        void ICanSetArchitecture.SetArchitecture(IArchitecture architecture) => m_Architecture = architecture;
        void ISystem.Init() => OnInit();
        protected abstract void OnInit();
    }

    #endregion

    #region Model 模型层

    public interface IModel : IBelongToArchitecture, ICanSetArchitecture, ICanGetUtility, ICanSendEvent
    {
        void Init();
    }

    public abstract class AbstractModel : IModel
    {
        IArchitecture m_Architecture;
        IArchitecture IBelongToArchitecture.GetArchitecture() => m_Architecture;
        void ICanSetArchitecture.SetArchitecture(IArchitecture architecture) => m_Architecture = architecture;
        void IModel.Init() => OnInit();
        protected abstract void OnInit();
    }

    #endregion

    #region Utility 工具层

    public interface IUtility { }

    #endregion

    #region Command 命令层

    public interface ICommand : IBelongToArchitecture, ICanSetArchitecture, ICanGetSystem, ICanGetModel, ICanGetUtility
                              , ICanSendCommand, ICanSendQuery, ICanSendEvent
    {
        void Execute();
    }

    public abstract class AbstractCommand : ICommand
    {
        IArchitecture m_Architecture;
        IArchitecture IBelongToArchitecture.GetArchitecture() => m_Architecture;
        void ICanSetArchitecture.SetArchitecture(IArchitecture architecture) => m_Architecture = architecture;
        void ICommand.Execute() => OnExecute();
        protected abstract void OnExecute();
    }

    #endregion

    #region Query 查询层

    public interface IQuery<TResult> : IBelongToArchitecture, ICanSetArchitecture, ICanGetSystem, ICanGetModel
                                     , ICanSendQuery
    {
        TResult Do();
    }

    public abstract class AbstractQuery<T> : IQuery<T>
    {
        IArchitecture m_Architecture;
        public IArchitecture GetArchitecture() => m_Architecture;
        public void SetArchitecture(IArchitecture architecture) => m_Architecture = architecture;
        public T Do() => OnDo();
        protected abstract T OnDo();
    }

    #endregion

    #region Rule 规则层

    public interface IBelongToArchitecture
    {
        IArchitecture GetArchitecture();
    }

    public interface ICanSetArchitecture
    {
        void SetArchitecture(IArchitecture architecture);
    }

    public interface ICanGetSystem : IBelongToArchitecture { }

    public static class CanGetSystemExtension
    {
        public static T GetSystem<T>(this ICanGetSystem self) where T : class, ISystem
        {
            return self.GetArchitecture().GetSystem<T>();
        }
    }

    public interface ICanGetModel : IBelongToArchitecture { }

    public static class CanGetModelExtension
    {
        public static T GetModel<T>(this ICanGetModel self) where T : class, IModel
        {
            return self.GetArchitecture().GetModel<T>();
        }
    }

    public interface ICanGetUtility : IBelongToArchitecture { }

    public static class CanGetUtilityExtension
    {
        public static T GetUtility<T>(this ICanGetUtility self) where T : class, IUtility
        {
            return self.GetArchitecture().GetUtility<T>();
        }
    }

    public interface ICanSendCommand : IBelongToArchitecture { }

    public static class CanSendCommandExtension
    {
        public static void SendCommand<T>(this ICanSendCommand self) where T : ICommand, new()
        {
            self.GetArchitecture().SendCommand<T>();
        }

        public static void SendCommand<T>(this ICanSendCommand self, T command) where T : ICommand
        {
            self.GetArchitecture().SendCommand<T>(command);
        }
    }

    public interface ICanSendQuery : IBelongToArchitecture { }

    public static class CanSendQueryExtension
    {
        public static TResult SendQuery<TResult>(this ICanSendQuery self, IQuery<TResult> query)
        {
            return self.GetArchitecture().SendQuery(query);
        }
    }

    public interface ICanRegisterEvent : IBelongToArchitecture { }

    public static class CanRegisterEventExtension
    {
        public static IUnRegister RegisterEvent<T>(this ICanRegisterEvent self, Action<T> onEvent)
        {
            return self.GetArchitecture().RegisterEvent<T>(onEvent);
        }

        public static void UnRegisterEvent<T>(this ICanRegisterEvent self, Action<T> onEvent)
        {
            self.GetArchitecture().UnRegisterEvent<T>(onEvent);
        }
    }

    public interface ICanSendEvent : IBelongToArchitecture { }

    public static class CanSendEventExtension
    {
        public static void SendEvent<T>(this ICanSendEvent self) where T : new()
        {
            self.GetArchitecture().SendEvent<T>();
        }

        public static void SendEvent<T>(this ICanSendEvent self, T e)
        {
            self.GetArchitecture().SendEvent<T>(e);
        }
    }

    #endregion

    #region TypeEventSystem

    public interface IUnRegister
    {
        void UnRegister();
    }

    public interface ITypeEventSystem
    {
        void Send<T>() where T : new();

        void Send<T>(T e);

        IUnRegister Register<T>(Action<T> onEvent);

        void UnRegister<T>(Action<T> onEvent);
    }

    public struct TypeEventSystemUnRegister<T> : IUnRegister
    {
        public ITypeEventSystem TypeEventSystem { get; set; }
        public Action<T> OnEvent { get; set; }

        public void UnRegister()
        {
            TypeEventSystem.UnRegister(OnEvent);

            TypeEventSystem = null;

            OnEvent = null;
        }
    }

    public class UnRegisterOnDestroyTrigger : MonoBehaviour
    {
        HashSet<IUnRegister> m_UnRegisters = new HashSet<IUnRegister>();

        public void AddUnRegister(IUnRegister unRegister)
        {
            m_UnRegisters.Add(unRegister);
        }

        void OnDestroy()
        {
            foreach (var unRegister in m_UnRegisters)
            {
                unRegister.UnRegister();
            }

            m_UnRegisters.Clear();
        }
    }

    public static class UnRegisterExtension
    {
        public static void UnRegisterWhenGameObjectDestroyed(this IUnRegister unRegister, GameObject gameObject)
        {
            var trigger = gameObject.GetComponent<UnRegisterOnDestroyTrigger>();

            if (!trigger)
            {
                trigger = gameObject.AddComponent<UnRegisterOnDestroyTrigger>();
            }

            trigger.AddUnRegister(unRegister);
        }
    }

    public class TypeEventSystem : ITypeEventSystem
    {
        public interface IRegistrations { }

        public class Registrations<T> : IRegistrations
        {
            public Action<T> OnEvent = e => { };
        }

        Dictionary<Type, IRegistrations> m_EventRegistration = new Dictionary<Type, IRegistrations>();

        public static readonly TypeEventSystem Global = new TypeEventSystem();

        public void Send<T>() where T : new()
        {
            var e = new T();
            Send<T>(e);
        }

        public void Send<T>(T e)
        {
            var type = typeof(T);
            if (m_EventRegistration.TryGetValue(type, out var eventRegistrations))
            {
                (eventRegistrations as Registrations<T>)?.OnEvent.Invoke(e);
            }
        }

        public IUnRegister Register<T>(Action<T> onEvent)
        {
            var type = typeof(T);
            if (m_EventRegistration.TryGetValue(type, out var eventRegistrations)) { }
            else
            {
                eventRegistrations = new Registrations<T>();
                m_EventRegistration.Add(type, eventRegistrations);
            }

            (eventRegistrations as Registrations<T>).OnEvent += onEvent;

            return new TypeEventSystemUnRegister<T>()
            {
                OnEvent = onEvent, TypeEventSystem = this
            };
        }

        public void UnRegister<T>(Action<T> onEvent)
        {
            var type = typeof(T);
            if (m_EventRegistration.TryGetValue(type, out var eventRegistrations))
            {
                (eventRegistrations as Registrations<T>).OnEvent -= onEvent;
            }
        }
    }

    public interface IOnEvent<T>
    {
        void OnEvent(T e);
    }

    public static class OnGlobalEventExtension
    {
        public static IUnRegister RegisterEvent<T>(this IOnEvent<T> self) where T : struct
        {
            return TypeEventSystem.Global.Register<T>(self.OnEvent);
        }

        public static void UnRegisterEvent<T>(this IOnEvent<T> self) where T : struct
        {
            TypeEventSystem.Global.UnRegister<T>(self.OnEvent);
        }
    }

    #endregion

    #region IOC

    public class IOCContainer
    {
        // 存储实例的字典
        Dictionary<Type, object> m_Instances = new Dictionary<Type, object>();

        // 注册实例的方法
        public void Register<T>(T instance)
        {
            var key = typeof(T);

            if (m_Instances.ContainsKey(key))
            {
                m_Instances[key] = instance;
            }
            else
            {
                m_Instances.Add(key, instance);
            }
        }

        // 获取实例的方法
        public T Get<T>() where T : class
        {
            var key = typeof(T);

            if (m_Instances.TryGetValue(key, out var retInstance))
            {
                return retInstance as T;
            }

            return null;
        }
    }

    #endregion

    #region BindableProperty

    public class BindableProperty<T>
    {
        public BindableProperty(T defaultValue = default)
        {
            m_Value = defaultValue;
        }

        T m_Value = default(T);

        public T Value
        {
            get => m_Value;
            set
            {
                if (value == null && m_Value == null) return;
                if (value != null && value.Equals(m_Value)) return;

                m_Value = value;
                m_OnValueChanged?.Invoke(value);
            }
        }

        Action<T> m_OnValueChanged = (v) => { };

        public IUnRegister Register(Action<T> onValueChanged)
        {
            m_OnValueChanged += onValueChanged;
            return new BindablePropertyUnRegister<T>()
            {
                BindableProperty = this
              , OnValueChanged = onValueChanged
            };
        }

        public IUnRegister RegisterWithInitValue(Action<T> onValueChanged)
        {
            onValueChanged(m_Value);
            return Register(onValueChanged);
        }

        public static implicit operator T(BindableProperty<T> property)
        {
            return property.Value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public void UnRegister(Action<T> onValueChanged)
        {
            m_OnValueChanged -= onValueChanged;
        }
    }

    public class BindablePropertyUnRegister<T> : IUnRegister
    {
        public BindableProperty<T> BindableProperty { get; set; }
        public Action<T> OnValueChanged { get; set; }

        public void UnRegister()
        {
            BindableProperty.UnRegister(OnValueChanged);
            BindableProperty = null;
            OnValueChanged = null;
        }
    }

    #endregion
}