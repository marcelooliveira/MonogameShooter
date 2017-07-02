using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Core;
using Microsoft.Xna.Framework.Content;
using ScreenControlsSample;
using BaseVerticalShooter.Input;
using BaseVerticalShooter.GameModel;
using Microsoft.Xna.Framework;
using BaseVerticalShooter.Core;

namespace BaseVerticalShooter.Core
{
    /// <summary>
    /// Resolutor de instâncias por inversão de controle.
    /// </summary>
    public class BaseResolver
    {
        #region Fields

        protected ContainerBuilder _Builder;
        private IContainer Container;
        private object ThreadSyncroot = new Object();
        private static BaseResolver instance;

        #endregion

        #region Properties

        public static BaseResolver Instance
        {
            get
            {
                if (instance == null)
                    instance = new BaseResolver();

                return instance;
            }
        }

        /// <summary>
        /// Obtém o construtor de containers (e cria um novo, se necessário) para o registro dos componentes.
        /// </summary>
        private ContainerBuilder Builder
        {
            get
            {
                lock (ThreadSyncroot)
                {
                    if (_Builder == null)
                    {
                        _Builder = new ContainerBuilder();
                    }

                    return _Builder;
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Registra a interface <typeparamref name="TInterfaceType"/> para o <paramref name="delegate"/> informado.
        /// </summary>
        /// <typeparam name="TInterfaceType">Tipo da interface a registrar.</typeparam>
        /// <param name="delegate">Delegate para instanciação da <typeparamref name="TInterfaceType">interface</typeparamref> em uma classe concreta.</param>
        public void Register<TInterfaceType>(Func<IComponentContext, TInterfaceType> @delegate)
        {
            Builder.Register(@delegate).As<TInterfaceType>();
            
        }

        /// <summary>
        /// Registra o tipo <typeparamref name="TConcreteType"/> para ser retornado quando a interface <typeparamref name="TInterfaceType"/> for requisitada.
        /// </summary>
        /// <typeparam name="TInterfaceType">Tipo da interface a registrar.</typeparam>
        /// <typeparam name="TConcreteType">Tipo da classe concreta para instanciação.</typeparam>
        public void Register<TInterfaceType, TConcreteType>() where TInterfaceType : class
        {
            lock (ThreadSyncroot)
            {
                Builder.RegisterType<TConcreteType>().As<TInterfaceType>();
            }
        }

        /// <summary>
        /// Registra o tipo <typeparamref name="TConcreteType"/> para ser retornado quando a interface <typeparamref name="TInterfaceType"/> for requisitada.
        /// </summary>
        /// <typeparam name="TInterfaceType">Tipo da interface a registrar.</typeparam>
        /// <typeparam name="TConcreteType">Tipo da classe concreta para instanciação.</typeparam>
        public void RegisterSingleton<TInterfaceType, TConcreteType>() where TInterfaceType : class
        {
            lock (ThreadSyncroot)
            {
                Builder.RegisterType<TConcreteType>().As<TInterfaceType>().SingleInstance();
            }
        }

        /// <summary>
        /// Registra o tipo <typeparamref name="TConcreteType"/> para ser retornado quando a interface <typeparamref name="TInterfaceType"/> for requisitada.
        /// </summary>
        /// <typeparam name="TInterfaceType">Tipo da interface a registrar.</typeparam>
        /// <typeparam name="TConcreteType">Tipo da classe concreta para instanciação.</typeparam>
        public void RegisterInstance<TInterfaceType>(TInterfaceType interfaceInstance) where TInterfaceType : class
        {
            lock (ThreadSyncroot)
            {
                Builder.RegisterInstance<TInterfaceType>(interfaceInstance);
            }
        }

        /// <summary>
        /// Instancia a classe concreta registrada para a interface <typeparamref name="TInterfaceType"/>.
        /// </summary>
        /// <typeparam name="TInterfaceType">Tipo da interface registrada.</typeparam>
        /// <returns>Instância da classe concreta.</returns>
        public TInterfaceType Resolve<TInterfaceType>() where TInterfaceType : class
        {
            lock (ThreadSyncroot)
            {
                if (Container == null)
                {
                    Container = Builder.Build();
                }
                return Container.Resolve<TInterfaceType>();
            }
        }

        /// <summary>
        /// Instancia a classe concreta registrada para a interface <typeparamref name="TInterfaceType"/>.
        /// </summary>
        /// <typeparam name="TInterfaceType">Tipo da interface registrada.</typeparam>
        /// <typeparam name="TConstructorParameter">Parâmetro do construtor da classe.</typeparam>
        /// <returns>Instância da classe concreta.</returns>
        public TInterfaceType Resolve<TInterfaceType, TConstructorParameter>(TConstructorParameter parameterValue)
        {
            lock (ThreadSyncroot)
            {
                if (Container == null)
                {
                    Container = Builder.Build();
                }

                var parameter = Autofac.TypedParameter.From<TConstructorParameter>(parameterValue);
                return Container.Resolve<TInterfaceType>(parameter);
            }
        }
        
        public void Reset()
        {
            _Builder = null;
            Container = null;
        }

        public virtual void RegisterGame(ContentManager content, IScreenPad screenPad)
        {
            RegisterInstance<IScreenPad>(screenPad);
            RegisterInstance<ICamera2d>(new Camera2d());
        }

        public void RegisterAll()
        {
            BaseResolver.Instance.Register<ILineProcessor, LineProcessor>();
            BaseResolver.Instance.Register<IFunctionExecutor, FunctionExecutor>();
            BaseResolver.Instance.Register<IReviewHelper, ReviewHelper>();
        }

        //public void RegisterObjects()
        //{
        //    var Objects = System.Reflection.Assembly.Load("Objects");

        //    Builder.RegisterAssemblyTypes(Objects)
        //        .Where(t => t.Name.EndsWith("Manager"))
        //        .AsImplementedInterfaces();

        //    Builder.RegisterAssemblyTypes(Objects)
        //        .Where(t => t.Name.EndsWith("Data"))
        //        .AsImplementedInterfaces();

        //    Builder.RegisterAssemblyTypes(Objects)
        //        .Where(t => t.Name.EndsWith("Cache"))
        //        .AsImplementedInterfaces();

        //    Builder.RegisterAssemblyTypes(Objects)
        //       .Where(t => t.Name.EndsWith("Search"))
        //       .AsImplementedInterfaces();

        //    Builder.RegisterAssemblyTypes(Objects)
        //       .Where(t => t.Name.EndsWith("Authorization"))
        //       .AsImplementedInterfaces();           
        //}

        //public void RegisterQueue()
        //{
        //    var Objects = System.Reflection.Assembly.Load("Queue");

        //    Builder.RegisterAssemblyTypes(Objects)
        //        .Where(t => t.Name.Contains("Queue"))
        //        .AsImplementedInterfaces();
        //}      

        public virtual IEnemy ResolveEnemy(int enemyIndex, Vector2 position, int groupId)
        {
            enemyIndex = enemyIndex % 10;
            return (IEnemy)Activator.CreateInstance(Type.GetType(string.Format("Shooter.GameModel.Enemy{0}", enemyIndex)), position, groupId);
        }

        #endregion
    }
}
