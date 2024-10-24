using Delegates.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delegates.Reports {

    // определяем делегаты    
    public delegate string MakeCaption(string caption); // делегат создания заголовка

    public delegate string BeginList(); // делегат начала списка
    
    public delegate string MakeItem(string valueType, string entry); // делегат создания элемента списка
    
    public delegate string EndList(); // делегат окончания списка    
    
    public delegate object MakeStatistics(IEnumerable<double> data); // делегат вычисления статистики

    public abstract class Formalization { // абстрактный класс определения формализации отчета
        public MakeCaption MakeCaption { get; protected set; } // делегат создания заголовка
        public BeginList BeginList { get; protected set; } // делегат начала списка
        public MakeItem MakeItem { get; protected set; } // делегат создания элемента списка
        public EndList EndList { get; protected set; } // делегат окончания списка    
    }

    public class HtmlFormalization : Formalization { // класс форматирования отчета в HTML
        public HtmlFormalization() { // конструктор класса делегатов, в соответствии с HTML        
            
            MakeCaption = caption => $"<h1>{caption}</h1>"; // делегат создания заголовка
            
            BeginList = () => "<ul>"; // делегат начала списка
            
            MakeItem = (valueType, entry) => $"<li><b>{valueType}</b>: {entry}"; // делегат создания элемента списка
            
            EndList = () => "</ul>"; // делегат окончания списка
        }
    }
    
    public class MarkDownFormalization : Formalization { // класс форматирования отчета в Markdown
        public MarkDownFormalization() { // конструктор класса

            MakeCaption = caption => $"## {caption}\n\n"; // метод, заголовок в markdown, лямбда-функция, возвращает строку с заголовком

            BeginList = () => ""; // метод, возвращает пустую строку
            
            MakeItem = (valueType, entry) => $" * **{valueType}**: {entry}\n\n"; // метод, элемент списка в markdown, лямбда-функция, возвращает строку с элементом списка
            
            EndList = () => ""; // метод, возвращает пустую строку
        }
    }

    public abstract class Statistics { // базовый класс подсчёта статистики
        public string Caption { get; protected set; } // поле названия статистики
        public MakeStatistics MakeStatistics { get; protected set; }// поле делегата вычисления статистики
    }

    public class MeanAndStdStatistics : Statistics { // класс, вывод среднего и стандартного отклонения
        public MeanAndStdStatistics() { // конструктор класса
            Caption = "Mean and Std"; // установка заголовка

            MakeStatistics = (data) => { // установка делегата
                var listData = data.ToList(); // конвертируем переданные данные в список

                var mean = listData.Average(); // среднее значение 
                var std = Math.Sqrt(listData.Select(z => Math.Pow(z - mean, 2)).Sum() / (listData.Count - 1)); // стандартное отклонение

                return new MeanAndStd {
                    Mean = mean, // среднее значение
                    Std = std // стандартное отклонение
                };
            };
        }
    }

    public class MedianStatistics : Statistics { // класс для вычисления медианы
        public MedianStatistics() { // конструктор             
            Caption = "Median"; // заголовок статистики

            MakeStatistics = (data) => { // функция вычисления медианы
                var sortedData = data.OrderBy(z => z); // Сортируем данные по возрастанию

                var middleIndex = sortedData.Count() / 2; // индекс середнего элемента

                var median = (sortedData.ElementAt(middleIndex) + (sortedData.Count() % 2 == 0 ? sortedData.ElementAt(middleIndex - 1) : 0)) / 2; // медиана
                
                return median; // возвращаем медиану
            };
        }
    }

    public class ReportMaker { // класс, создание отчетов
        public string MakeReport(IEnumerable<Measurement> measurements, Statistics statistics, Formalization formalization) { // метод для создания отчета

            var result = new StringBuilder(); // объект для хранения результата

            result.Append(formalization.MakeCaption(statistics.Caption)); // добавляем заголовок из статистики

            result.Append(formalization.BeginList()); // начинаем список

            AppendMeasurementStatistic(result, "Temperature", measurements, statistics, z => z.Temperature, formalization); // добавляем статистику по температуре

            AppendMeasurementStatistic(result, "Humidity", measurements, statistics, z => z.Humidity, formalization); // добавляем к списку статистику по влажности

            result.Append(formalization.EndList()); // заканчиваем список

            return result.ToString(); // возвращаем результат
        }

        private void AppendMeasurementStatistic(StringBuilder result, string measurementName,
                                                 IEnumerable<Measurement> measurements,
                                                 Statistics statistics,
                                                 Func<Measurement, double> measurementSelector,
                                                 Formalization formalization) { // метод добавления статистики по конкретному замеру
 
            result.Append(formalization.MakeItem(measurementName,
                statistics.MakeStatistics(measurements.Select(measurementSelector)).ToString())); // добавляем элемент с названием и значением статистики из замеров данных 
        }
    }

    public static class ReportMakerHelper { // описываем статический класс для создания отчетов разного вида
        
        public static string MeanAndStdHtmlReport(IEnumerable<Measurement> data) { // метод для создания отчета в HTML со средним значением и стандартным отклонением
            return new ReportMaker().MakeReport(data, new MeanAndStdStatistics(), new HtmlFormalization());
        }

        public static string MedianMarkdownReport(IEnumerable<Measurement> data) { // метод для создания отчета в Markdown с медианой
            return new ReportMaker().MakeReport(data, new MedianStatistics(), new MarkDownFormalization());
        }
        
        public static string MeanAndStdMarkdownReport(IEnumerable<Measurement> measurements) { // метод для создания отчета в Markdown со средним значением и стандартным отклонением
            return new ReportMaker().MakeReport(measurements, new MeanAndStdStatistics(), new MarkDownFormalization());
        }

        public static string MedianHtmlReport(IEnumerable<Measurement> measurements) { // метод для создания отчета в HTML с медианой
            return new ReportMaker().MakeReport(measurements, new MedianStatistics(), new HtmlFormalization());
        }
    }
}