namespace bing_search_dotnet
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    class ExampleAbstraction
    {
        public IList<Example> Examples { get; private set; }

        private TypeInfo backingType;

        public ExampleAbstraction(TypeInfo backingType)
        {
            this.backingType = backingType;
        }

        public void Load()
        {
            this.Examples = new List<Example>();

            var exampleMethods = backingType.GetMethods().Where(method => method.GetCustomAttribute<ExampleAttribute>() != null).ToArray();

            for (var i = 0; i < exampleMethods.Length; i++)
            {
                var methodInfo = exampleMethods[i];
                var exampleMethod = methodInfo.GetCustomAttribute<ExampleAttribute>();
                this.Examples.Add(new Example(i, exampleMethod.ExampleDescription, methodInfo));

                // assert our assumptions about the method so that this throws if someone adds an improper signature
                if (!methodInfo.IsStatic)
                {
                    throw new TypeAccessException(string.Format("Method with description \"{0}\" needs to be static", exampleMethod.ExampleDescription));
                }

                var methodParams = methodInfo.GetParameters();
                if ((methodParams == null || methodParams.Length != 1 || methodParams[0].ParameterType != typeof(string)))
                {
                    throw new TypeAccessException(string.Format("Method with description \"{0}\" needs to have one string parameter for the subscription key", exampleMethod.ExampleDescription));
                }
            }
        }

        internal class Example
        {
            public int Number { get; private set; }

            public string Description { get; private set; }

            private MethodInfo method;

            public Example(int number, string description, MethodInfo method)
            {
                this.Number = number;
                this.Description = description;
                this.method = method;
            }

            public void Invoke(string subscriptionKey)
            {
                this.method.Invoke(null, new object[] { subscriptionKey });
            }
        }
    }
}
