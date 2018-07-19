namespace Microsoft.Azure.CognitiveServices.LUIS.Authoring.Sample
{
    using DotSpinners;
    using EasyConsole;
    using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring;
    using Newtonsoft.Json;
    using System;
    using System.Threading.Tasks;

    class BaseMenuPage : MenuPage
    {
        public LUISAuthoringClient Client { get; private set; }

        public BaseMenuPage(string title, BaseProgram program, params Option[] options) : base(title, program, options)
        {
            Client = program.Client;
        }

        protected void Print(object obj)
        {
            var json = JsonConvert.SerializeObject(obj, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            Console.WriteLine(json);
        }

        protected T AwaitTask<T>(Task<T> task, bool clearAfterRun = false)
        {
            var awaiter = new DotSpinner(SpinnerTypes.Ping, task).Center();
            awaiter.Start();
            if (clearAfterRun)
            {
                Console.Clear();
                base.Display();
            }
            else
            {
                Console.WriteLine();
            }
            return task.Result;
        }

        protected void AwaitTask(Task task, bool clearAfterRun = false)
        {
            var awaiter = new DotSpinner(SpinnerTypes.Ping, task).Center();
            awaiter.Start();
            if (clearAfterRun)
            {
                Console.Clear();
                base.Display();
            }
            else
            {
                Console.WriteLine();
            }
        }
        protected void WaitForGoBack()
        {
            Input.ReadString("Press any key to go back");
            Program.NavigateBack();
        }

        protected void WaitForNavigateTo<T>() where T : Page
        {
            Input.ReadString("Press any key to continue");
            Program.NavigateTo<T>();
        }

        protected void WaitForNavigateHome()
        {
            Input.ReadString("Press any key to go home");
            Program.NavigateHome();
        }

        protected T NavigateWithInitializer<T>(Action<T> initializer) where T : Page
        {
            var page = Program.SetPage<T>();
            initializer(page);

            Console.Clear();
            page.Display();

            return page;
        }

        protected void SafeAddToMenu(Option option)
        {
            if (!Menu.Contains(option.Name))
            {
                Menu.Add(option);
            }
        }

        protected void SafeAddToMenu(string name, Action callback)
        {
            SafeAddToMenu(new Option(name, callback));
        }
    }
}
