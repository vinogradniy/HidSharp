using System;
using System.Threading;

using FluentAssertions;

using HidSharp;
using HidSharp.Utility;

using NUnit.Framework;

namespace UnitTestsCommon.Adapters.BaseAdapter
{
    /// <summary>
    /// Тестирование класса UsbAdapterUtils c HidSharp. 
    /// </summary>    
    [TestFixture(Description = "Test Class UsbAdapterUtils")]
    public class HidSharpUnitTests
    {
        #region TestFixtureSetUp

        ///<summary>
        /// До выполнения тестов.
        ///</summary>
        [OneTimeSetUp]
        public void FixtureSetUp()
        {
            HidSharpDiagnostics.EnableTracing = true;
            HidSharpDiagnostics.PerformStrictChecks = true;
        }

        ///<summary>
        /// После выполнения тестов.
        ///</summary>
        [OneTimeTearDown]
        public void FixtureTearDown()
        {
            HidSharpDiagnostics.EnableTracing = false;
            HidSharpDiagnostics.PerformStrictChecks = false;
        }

        #endregion

        #region SetUp

        ///<summary>
        /// До выполнения теста.
        ///</summary>
        [SetUp]
        public void SetUp()
        {
        }

        ///<summary>
        /// После выполнения теста.
        ///</summary>
        [TearDown]
        public void TearDown()
        {
        }

        #endregion

        #region Test methods

        ///<summary>
        /// Тестовый метод.
        ///</summary>      
        [Test(Description = "Тестирование метода ...")]
        [RequiresThread(ApartmentState.STA)]
        //[Repeat(100)]
        public void TestHidSharpFixUnloadAppDomain()
        {
            for (var i = 0; i < 100; i++)
            {
                Console.WriteLine(i + 1);

                var setup = new AppDomainSetup
                                           {
                                               ApplicationBase = GetApplicationDirectory(),
                                           };
                var appDomain = AppDomain.CreateDomain($"Loading Domain {Guid.NewGuid():N}", null, setup);
                var program = (Launcher)appDomain.CreateInstanceAndUnwrap(typeof(Launcher).Assembly.FullName, typeof(Launcher).FullName);

                program.Invoking(launcher => launcher.Execute()).Should().NotThrow(); // исполняем код в другом домене приложения
                this.Invoking(tests => AppDomain.Unload(appDomain)).Should().NotThrow(); // ssdi: не должно быть исключений, домен должен выгружаться успешно, иначе тесты на сервере не будут проходить
            }
        }

        /// <summary>
        /// Возвращает базовый каталог, в котором распознаватель сборок производит поиск.
        /// Путь оканчивается обратным слэшем.
        /// </summary>
        /// <remarks>
        /// Корректно работает для сервисов и вслучае, если переопределена текущая директория.
        /// Например если приложение запускается по ярлыку.
        /// </remarks>
        /// <returns>
        /// Полный путь (с обратным слешем на конце), по которому размещается исполняемый файл сервера.
        /// </returns>
        private static string GetApplicationDirectory()
        {
            //Возвращает базовый каталог, в котором распознаватель сборок производит поиск 
            var directory = AppDomain.CurrentDomain.BaseDirectory;

            //Обратный слеш уже есть, если директорий является рутовым директорием диска
            if (!directory.EndsWith(@"\"))
            {
                directory += @"\";
            }

            return directory;
        }

        #endregion
    }

    public class Launcher : MarshalByRefObject
    {
        /// <summary>
        /// This gets executed in the temporary appdomain.
        /// No error handling to simplify demo.
        /// </summary>
        public void Execute()
        {
            Console.WriteLine($"Test output");
            
            var hids = DeviceList.Local.GetHidDevices();
            foreach (var hidDevice in hids)
            {
                Console.WriteLine($"/{hidDevice.VendorID}/{hidDevice.ProductID}/{hidDevice.DevicePath}");
            }
        }
    }
}
