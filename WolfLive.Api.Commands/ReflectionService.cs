using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WolfLive.Api.Commands
{
    /// <summary>
    /// Providers useful helper methods for reflection based actions
    /// </summary>
    public interface IReflectionService
    {
        /// <summary>
        /// Gets any types the implement the given interface
        /// </summary>
        /// <param name="implementedtype">The interface to check for</param>
        /// <returns>The types that implement the given interface</returns>
        IEnumerable<Type> GetTypes(Type implementedInterface);

        /// <summary>
        /// Finds any type that matches the given predicate
        /// </summary>
        /// <param name="predicate">The predicate to check against</param>
        /// <returns>All of the types that match the given predicate</returns>
        IEnumerable<Type> GetTypes(Func<Type, bool> predicate);

        /// <summary>
        /// Does a safe type conversion from the given object to the given type
        /// </summary>
        /// <param name="obj">The type of object to convert</param>
        /// <param name="toType">The type to convert the object to</param>
        /// <returns>The converted object</returns>
        object ChangeType(object obj, Type toType);

        /// <summary>
        /// Does a safe type conversion from the given object to the given type
        /// </summary>
        /// <typeparam name="T">The type to convert the object to</typeparam>
        /// <param name="obj">The type of object to convert</param>
        /// <returns>The converted object</returns>
        T ChangeType<T>(object obj);

        /// <summary>
        /// Executes the given method and resolves any dependencies and parameters automatically
        /// </summary>
        /// <param name="info">The method to execute</param>
        /// <param name="def">The parent object to this method (null if the method is static)</param>
        /// <param name="provider">The service provider to resolve the dependencies from</param>
        /// <param name="error">Whether or not an error occurred while executing the method</param>
        /// <param name="defaultparameters">Any parameters that cannot be resolved via dependency injection</param>
        /// <returns>The return results of the executed method</returns>
        object ExecuteMethod(MethodInfo info, object def, IServiceProvider provider, out bool error, params object[] defaultparameters);

        /// <summary>
        /// Executes the given method using the parameters specified
        /// </summary>
        /// <param name="info">The method to execute</param>
        /// <param name="def">The parent object to execute the method with (null if static)</param>
        /// <param name="error">Whether or not an error occurred in while executing the method</param>
        /// <param name="pars">The parameters to execute the method with</param>
        /// <returns>The return result of the method</returns>
        object ExecuteMethod(MethodInfo info, object def, out bool error, params object[] pars);

        /// <summary>
        /// Executes the given method and automatically resolves the parent type and any dependencies
        /// </summary>
        /// <param name="info">The method to execute</param>
        /// <param name="provider">The service provider to resolve the dependencies from</param>
        /// <param name="error">Whether or not an error occurred while executing the method</param>
        /// <param name="pars">Any parameters that cannot be resolved via dependency injection</param>
        /// <returns>The return results of the executed method</returns>
        object ExecuteMethod(MethodInfo info, IServiceProvider provider, out bool error, params object[] pars);

        Task ExecuteMethod(MethodInfo info, IServiceProvider provider, params object[] pars);

        /// <summary>
        /// Creates a deep clone of the given object
        /// </summary>
        /// <typeparam name="T">The type of object to clone</typeparam>
        /// <param name="item">The object to clone</param>
        /// <returns>The cloned object</returns>
        T Clone<T>(T item);

        /// <summary>
        /// Creates an instance of the given type resolved through dependency injection if possible
        /// </summary>
        /// <typeparam name="T">The type of object to cast the result to</typeparam>
        /// <param name="type">The type of object to create</param>
        /// <param name="provider">The service provider to resolve an dependencies</param>
        /// <returns>The created and casted result</returns>
        T Create<T>(Type type, IServiceProvider provider);

        /// <summary>
        /// Creates an instance of the given type resolved through dependency injection if possible
        /// </summary>
        /// <param name="type">The type of object to create</param>
        /// <param name="provider">The service provider to resolve an dependencies</param>
        /// <returns>The created result</returns>
        object Create(Type type, IServiceProvider provider);
    }

    /// <summary>
    /// Providers useful helper methods for reflection based actions
    /// </summary>
    public class ReflectionService : IReflectionService
    {
        /// <summary>
        /// The default encoding to use when transforming strings to and from byte arrays
        /// </summary>
        public static Encoding Encoder = Encoding.UTF8;

        /// <summary>
        /// Does a safe type conversion from the given object to the given type
        /// </summary>
        /// <param name="obj">The type of object to convert</param>
        /// <param name="toType">The type to convert the object to</param>
        /// <returns>The converted object</returns>
        public virtual object ChangeType(object obj, Type toType)
        {
            if (obj == null)
                return null;

            var fromType = obj.GetType();

            var to = Nullable.GetUnderlyingType(toType) ?? toType;
            var from = Nullable.GetUnderlyingType(fromType) ?? fromType;

            if (to == from)
                return obj;

            if (to.IsEnum)
            {
                return Enum.ToObject(to, Convert.ChangeType(obj, to.GetEnumUnderlyingType()));
            }

            if (from == typeof(byte[]))
            {
                obj = Encoder.GetString((byte[])obj);

                if (to == typeof(string))
                    return obj;
            }

            if (to == typeof(byte[]) && from == typeof(string))
            {
                return Encoder.GetBytes((string)obj);
            }

            return Convert.ChangeType(obj, to);
        }

        /// <summary>
        /// Does a safe type conversion from the given object to the given type
        /// </summary>
        /// <typeparam name="T">The type to convert the object to</typeparam>
        /// <param name="obj">The type of object to convert</param>
        /// <returns>The converted object</returns>
        public virtual T ChangeType<T>(object obj)
        {
            return (T)ChangeType(obj, typeof(T));
        }

        /// <summary>
        /// Finds any type that matches the given predicate
        /// </summary>
        /// <param name="predicate">The predicate to check against</param>
        /// <returns>All of the types that match the given predicate</returns>
        public virtual IEnumerable<Type> GetTypes(Func<Type, bool> predicate)
        {
            var assembly = Assembly.GetEntryAssembly();
            var alreadyLoaded = new List<string>
            {
                assembly.FullName
            };

            foreach (var type in assembly.DefinedTypes)
            {
                if (predicate(type))
                    yield return type;
            }

            var assems = assembly.GetReferencedAssemblies()
                .Select(t => t.FullName)
                .Except(alreadyLoaded)
                .ToArray();
            foreach (var ass in assems)
            {
                foreach (var type in GetTypes(predicate, ass, alreadyLoaded))
                {
                    yield return type;
                }
            }
        }

        private IEnumerable<Type> GetTypes(Func<Type, bool> checks, string assembly, List<string> alreadyLoaded)
        {
            if (alreadyLoaded.Contains(assembly))
                yield break;

            alreadyLoaded.Add(assembly);
            var asml = Assembly.Load(assembly);
            foreach (var type in asml.DefinedTypes)
            {
                if (checks(type))
                    yield return type;
            }

            var assems = asml.GetReferencedAssemblies()
                .Select(t => t.FullName)
                .Except(alreadyLoaded)
                .ToArray();
            foreach (var ass in assems)
            {
                foreach (var type in GetTypes(checks, ass, alreadyLoaded))
                {
                    yield return type;
                }
            }
        }

        /// <summary>
        /// Gets any types the implement the given interface
        /// </summary>
        /// <param name="implementedtype">The interface to check for</param>
        /// <returns>The types that implement the given interface</returns>
        public virtual IEnumerable<Type> GetTypes(Type implementedtype)
        {
            return GetTypes((t) => t.GetTypeInfo().ImplementedInterfaces.Contains(implementedtype) &&
                                   !t.IsInterface && !t.IsAbstract);
        }

        /// <summary>
        /// Executes the given method and resolves any dependencies and parameters automatically
        /// </summary>
        /// <param name="info">The method to execute</param>
        /// <param name="def">The parent object to this method (null if the method is static)</param>
        /// <param name="provider">The service provider to resolve the dependencies from</param>
        /// <param name="error">Whether or not an error occurred while executing the method</param>
        /// <param name="defaultparameters">Any parameters that cannot be resolved via dependency injection</param>
        /// <returns>The return results of the executed method</returns>
        public virtual object ExecuteMethod(MethodInfo info, object def, IServiceProvider provider, out bool error, params object[] defaultparameters)
        {
            try
            {
                error = false;
                if (info == null)
                    return null;

                var pars = info.GetParameters();

                if (pars.Length <= 0)
                    return ExecuteMethod(info, def, out error);

                var args = new object[pars.Length];

                for (var i = 0; i < pars.Length; i++)
                {
                    var par = pars[i];

                    var pt = par.ParameterType;

                    var fit = defaultparameters.FirstOrDefault(t => t != null && pt.IsAssignableFrom(t.GetType()));

                    if (fit != null)
                    {
                        args[i] = fit;
                        continue;
                    }

                    var next = provider.GetService(pt);
                    if (next != null)
                    {
                        args[i] = next;
                        continue;
                    }

                    args[i] = pt.IsValueType ? Activator.CreateInstance(pt) : null;
                }

                return ExecuteMethod(info, def, out error, args);
            }
            catch (Exception ex)
            {
                error = true;
                return ex;
            }
        }

        /// <summary>
        /// Executes the given method using the parameters specified
        /// </summary>
        /// <param name="info">The method to execute</param>
        /// <param name="def">The parent object to execute the method with (null if static)</param>
        /// <param name="error">Whether or not an error occurred in while executing the method</param>
        /// <param name="pars">The parameters to execute the method with</param>
        /// <returns>The return result of the method</returns>
        public virtual object ExecuteMethod(MethodInfo info, object def, out bool error, params object[] pars)
        {
            try
            {
                error = false;
                return info.Invoke(def, pars);
            }
            catch (Exception ex)
            {
                error = true;
                return ex;
            }
        }

        /// <summary>
        /// Executes the given method and automatically resolves the parent type and any dependencies
        /// </summary>
        /// <param name="info">The method to execute</param>
        /// <param name="provider">The service provider to resolve the dependencies from</param>
        /// <param name="error">Whether or not an error occurred while executing the method</param>
        /// <param name="pars">Any parameters that cannot be resolved via dependency injection</param>
        /// <returns>The return results of the executed method</returns>
        public virtual object ExecuteMethod(MethodInfo info, IServiceProvider provider, out bool error, params object[] pars)
        {
            error = false;
            try
            {
                if (info == null)
                    return null;

                if (info.IsStatic)
                    return ExecuteMethod(info, null, provider, out error, pars);

                var def = Create(info.DeclaringType, provider);
                if (def == null)
                    throw new Exception($"Cannot create instance of: {info.DeclaringType.Name} for {info.Name}");

                if (def is WolfContext context)
				{
                    context.Client = pars.FirstOrDefault(t => t is IWolfClient) as IWolfClient; 
                    context.Command = pars.FirstOrDefault(t => t is CommandMessage) as CommandMessage;
				}

                return ExecuteMethod(info, def, provider, out error, pars);
            }
            catch (Exception ex)
            {
                error = true;
                return ex;
            }
        }

        public virtual async Task ExecuteMethod(MethodInfo info, IServiceProvider provider, params object[] pars)
		{
            var result = ExecuteMethod(info, provider, out bool error, pars);
            if (error || result is Exception)
                throw (Exception)result;

            if (info.ReturnType == typeof(void))
                return;

            if (result is Task ||
                result.GetType().GetGenericTypeDefinition() == typeof(Task<>))
			{
                dynamic taskdyn = result;
                await taskdyn;
			}
		}

        /// <summary>
        /// Creates a deep clone of the given object
        /// </summary>
        /// <typeparam name="T">The type of object to clone</typeparam>
        /// <param name="item">The object to clone</param>
        /// <returns>The cloned object</returns>
        public virtual T Clone<T>(T item)
        {
            var ser = JsonConvert.SerializeObject(item);
            return JsonConvert.DeserializeObject<T>(ser);
        }

        /// <summary>
        /// Creates an instance of the given type resolved through dependency injection if possible
        /// </summary>
        /// <typeparam name="T">The type of object to cast the result to</typeparam>
        /// <param name="type">The type of object to create</param>
        /// <param name="provider">The service provider to resolve an dependencies</param>
        /// <returns>The created and casted result</returns>
        public T Create<T>(Type type, IServiceProvider provider)
        {
            return (T)Create(type, provider);
        }

        /// <summary>
        /// Creates an instance of the given type resolved through dependency injection if possible
        /// </summary>
        /// <param name="type">The type of object to create</param>
        /// <param name="provider">The service provider to resolve an dependencies</param>
        /// <returns>The created result</returns>
        public object Create(Type type, IServiceProvider provider)
        {
            var output = provider.GetService(type);
            if (output != null)
                return output;

            var cons = type.GetConstructors((BindingFlags)(-1));

            if (cons.Length == 0)
                return Activator.CreateInstance(type);

            foreach (var con in cons)
            {
                try
                {
                    var pars = con.GetParameters();
                    var args = new object[pars.Length];

                    for (var i = 0; i < pars.Length; i++)
                    {
                        args[i] = provider.GetService(pars[i].ParameterType);
                    }

                    return Activator.CreateInstance(type, args);
                }
                catch { continue; }
            }

            return null;
        }
    }
}