using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SharedList
{
	/// <summary>
	/// В сборочном цеху завода, вокруг одного стола работает несколько роботов
	/// Один робот раскладывает по столу заготовки, еще несколько роботов собирают их.
	/// Роботы-сборщики берут деталь со стола и собирают ее в два этапа, сборка деталей моментальная, но после сборки роботу необходимо немного остыть.
	/// За отведенный период времени заготовки должны быть собраны. 
	/// 
	/// Доработать код так, чтобы бракованных изделий не было, а количество недоделанных свести к минимуму
	/// Как можно улучшить этот код? Какие проблемы вы заметили? Улучшайте!
	/// </summary>
	class Program
	{
        public static Mutex muWorker = new Mutex();
        static void Main(string[] args)
        
		{
            
			FrameProvider frameProvider = new FrameProvider();  //создаем объект Робот - поставщик рам (0 из вакуума)

			Assembler assembler = new Assembler(); // создаем объект Робот-сборщик


            Thread provider = new Thread(frameProvider.Start);  //запускаем поток метода СТАРТ поставщика рам
            provider.Start();  //страт потока


            //это наши роботы
            for (int i = 0; i < 5; i++)
			{
				new Thread(assembler.Start).Start(); //для каждого робота сборки.делаем метод Старт и Старт.потока

            }

            //пауза
			Thread.Sleep(10000);

            //это просто вывод
			ListManager.StopTrigger = true;
			Console.WriteLine(ListManager.GetInfo());
			Console.ReadLine();
			Console.ReadLine();
		}

		//Рама = 0,
		//Полусобрано = 1,
		//Собрано = 2,
		//Испорчено >= 3

		public static class ListManager  
		{
			public static bool StopTrigger = false; 
            public static List<int> SharedList; 

            static ListManager()   
			{
                SharedList = new List<int>();  
            }

			public static string GetInfo()   
			{
                //начальные данные
				string result = "";
				int frames = 0;
				int halfs = 0;
				int assembled = 0;
				int broken = 0;
                

                // проверяем все номера в шаредлисте (переменная типа ЛИСТ с интами)
				foreach (var element in SharedList)
				{
					switch (element) //смотрим на значение элемента в данном
					{
						case 0:
							{
								frames++; //рамы
								break;
							}

						case 1:
							{
								halfs++; //начато
								break;
							}

						case 2:
							{
								assembled++; //собрано.
								break;
							}
						default:
							{
								broken++; //значит сломался
								break;
							}
					}

                    
					result = String.Format("Итого:\r\n" +
										   "Рамы:{0}\r\n" +
										   "Полусобрано:{1}\r\n" +
										   "Собрано:{2}\r\n" +
										   "Испорчено:{3}\r\n",
						frames, halfs, assembled, broken
					); 
				}
                // вывод
				return result;
			}

//берет первый попавшийся элемент в списке со значением 0 или 1
			public static int GetIndex(int[] requiredConditions) 
			{
                muWorker.WaitOne();
                for (int i = 0; i < SharedList.Count; i++) 
				{
                                     
                    if (requiredConditions.Contains(SharedList[i])) 
                         {
                        muWorker.ReleaseMutex();
                        return i;
                        
                         }

				}
                muWorker.ReleaseMutex();
                return -1; //
                
            }
		}

		public class FrameProvider : Robot //поставщик рам. здесь мы. опять же от Робота. 
		{
			
			public override void DoWork()
			{
				if (ListManager.SharedList.Count < 150)  
					ListManager.SharedList.Add(0);      //добавляем 0 (вероятно, раму).
			}
		}

		//Сборщики должны брать в работу только пустые рамы или недоделанные изделия
		public class Assembler : Robot 
		{                              
			public override void DoWork() 
			{                              
                
				int index = ListManager.GetIndex(new[] { 0, 1 }); //индекс - метод гетиндекс в классе шаредлист
                                                
                if (index >= 0) 
                {
                    ListManager.SharedList[index]++; 
                    Thread.Sleep(250);              
                    
                }
                

            }
		}

		public abstract class Robot //класс робот
		{
			public void Start() //  навание МЕТОДА старт:
			{                   // и описание (что он делает)
				while (!ListManager.StopTrigger) //пока в другом классе ЛИСТМАНАГЕР не будет сигнала СТОП
				{
					DoWork();  // делать некую мифическую работу... метод ДУВОРК
				}
			}                  

			public abstract void DoWork(); // здесь, МЕТОД ДУВОРК пустой. 
		}
	}
}