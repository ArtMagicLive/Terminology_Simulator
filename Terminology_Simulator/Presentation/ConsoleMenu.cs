using System.Net.NetworkInformation;
using Terminology_Simulator.Models;
using Terminology_Simulator.Services;

namespace Terminology_Simulator.Presentation;

/// <summary>
/// Provides an interactive console user interface for the terminology trainer.
/// </summary>
public class ConsoleMenu
{
    private readonly TermManager _termManager;
    private readonly TrainingService _trainingService;

    public ConsoleMenu(TermManager termManager, TrainingService trainingService)
    {
        _termManager = termManager;
        _trainingService = trainingService;
    }

    public void Run()
    {
        bool exitRequested = false;

        while (!exitRequested)
        {
            ShowHeader("Главное меню");
            Console.WriteLine("1. Управление наборами");
            Console.WriteLine("2. Поиск");
            Console.WriteLine("3. Тренировка");
            Console.WriteLine("4. Статистика");
            Console.WriteLine("0. Выход");

            switch (ReadMenuChoice("Выберите пункт: "))
            {
                case "1":
                    ManageSetsMenu();
                    break;
                case "2":
                    SearchMenu();
                    break;
                case "3":
                    TrainingMenu();
                    break;
                case "4":
                    ShowStatistics();
                    break;
                case "0":
                    exitRequested = true;
                    break;
                default:
                    ShowMessage("Неизвестный пункт меню.");
                    break;
            }
        }
    }

    private void ManageSetsMenu()
    {
        bool backRequested = false;

        while (!backRequested)
        {
            ShowHeader("Управление наборами");
            Console.WriteLine("1. Показать наборы");
            Console.WriteLine("2. Создать набор");
            Console.WriteLine("3. Открыть набор");
            Console.WriteLine("4. Переименовать набор");
            Console.WriteLine("5. Удалить набор");
            Console.WriteLine("0. Назад");

            switch (ReadMenuChoice("Выберите пункт: "))
            {
                case "1":
                    PrintTermSets();
                    Pause();
                    break;
                case "2":
                    CreateSet();
                    break;
                case "3":
                    TermSet? selectedSet = SelectTermSet();
                    if (selectedSet is not null)
                    {
                        ManageTermsMenu(selectedSet);
                    }
                    break;
                case "4":
                    RenameSet();
                    break;
                case "5":
                    DeleteSet();
                    break;
                case "0":
                    backRequested = true;
                    break;
                default:
                    ShowMessage("Неизвестный пункт меню.");
                    break;
            }
        }
    }

    private void ManageTermsMenu(TermSet termSet)
    {
        bool backRequested = false;

        while (!backRequested)
        {
            ShowHeader($"Набор: {termSet.Name}");
            Console.WriteLine("1. Показать термины");
            Console.WriteLine("2. Добавить термин");
            Console.WriteLine("3. Редактировать термин");
            Console.WriteLine("4. Удалить термин");
            Console.WriteLine("5. Добавить вариант ответа");
            Console.WriteLine("6. Удалить вариант ответа");
            Console.WriteLine("0. Назад");

            switch (ReadMenuChoice("Выберите пункт: "))
            {
                case "1":
                    PrintTerms(termSet);
                    Pause();
                    break;
                case "2":
                    AddTerm(termSet);
                    break;
                case "3":
                    EditTerm(termSet);
                    break;
                case "4":
                    DeleteTerm(termSet);
                    break;
                case "5":
                    AddDefinition(termSet);
                    break;
                case "6":
                    RemoveDefinition(termSet);
                    break;
                case "0":
                    backRequested = true;
                    break;
                default:
                    ShowMessage("Неизвестный пункт меню.");
                    break;
            }
        }
    }

    private void SearchMenu()
    {
        ShowHeader("Поиск");
        string query = ReadRequiredText("Введите слово для поиска: ");
        List<KeyValuePair<Term, TermSet>> results = _termManager.SearchTerms(query);

        if (results.Count == 0)
        {
            Console.WriteLine("Термин не найден. Создать новый? (Y/N)");
            string answer = ReadMenuChoice("Ваш выбор: ");

            if (answer.Equals("Y", StringComparison.OrdinalIgnoreCase))
            {
                CreateTermFromSearch(query);
            }

            return;
        }

        for (int i = 0; i < results.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {results[i].Key.Word} ({results[i].Value.Name})");
        }

        int selectedIndex = ReadNumber("Выберите найденный термин: ", 1, results.Count) - 1;
        KeyValuePair<Term, TermSet> selectedResult = results[selectedIndex];
        ShowFoundTermContextMenu(selectedResult.Key, selectedResult.Value);
    }

    private void ShowFoundTermContextMenu(Term term, TermSet termSet)
    {
        bool exitRequested = false;

        while (!exitRequested)
        {
            ShowHeader($"Найден термин: {term.Word}");
            PrintTerm(term);
            Console.WriteLine();
            Console.WriteLine("1. Редактировать термин");
            Console.WriteLine("2. Удалить");
            Console.WriteLine("3. Добавить вариант ответа");
            Console.WriteLine("0. Выйти в главное меню");

            switch (ReadMenuChoice("Выберите пункт: "))
            {
                case "1":
                    EditTerm(termSet, term);
                    break;
                case "2":
                    DeleteTerm(termSet, term);
                    exitRequested = true;
                    break;
                case "3":
                    AddDefinition(termSet, term);
                    break;
                case "0":
                    exitRequested = true;
                    break;
                default:
                    ShowMessage("Неизвестный пункт меню.");
                    break;
            }
        }
    }

    private void TrainingMenu()
    {
        ShowHeader("Тренировка");

        if (_termManager.TermSets.Count == 0)
        {
            ShowMessage("Сначала создайте хотя бы один набор терминов.");
            return;
        }

        Console.WriteLine("1. Выбрать тему");
        Console.WriteLine("2. Смешанный режим");
        Console.WriteLine("0. Назад");
        string mode = ReadMenuChoice("Выберите режим: ");

        if (mode == "0")
        {
            return;
        }

        bool mixed = mode == "2";
        List<Guid> setIds = new();
        string themeName = "Смешанный режим";

        if (!mixed)
        {
            TermSet? selectedSet = SelectTermSet();
            if (selectedSet is null)
            {
                return;
            }

            setIds.Add(selectedSet.Id);
            themeName = selectedSet.Name;
        }

        int questionCount = ReadNumber("Количество вопросов (1-20): ", 1, 20);
        TrainingSession session = _trainingService.StartSession(setIds, questionCount, mixed);

        if (session.TotalQuestions == 0)
        {
            ShowMessage("В выбранных наборах нет терминов для тренировки.");
            return;
        }

        while (!session.IsFinished)
        {
            Term question = session.GetCurrentQuestion();
            Console.WriteLine();
            Console.WriteLine($"Термин: {question.Word}");
            string answer = ReadRequiredText("Ваш ответ: ");
            bool isCorrect = session.SubmitAnswer(answer);
            Console.WriteLine(isCorrect ? "Верно!" : "Неверно.");
        }

        SessionResult result = new()
        {
            DateTime = DateTime.Now,
            ThemeName = themeName,
            TotalQuestions = session.TotalQuestions,
            CorrectAnswers = session.CorrectCount,
            SuccessRate = session.TotalQuestions == 0 ? 0 : session.CorrectCount * 100.0 / session.TotalQuestions
        };

        Dictionary<Guid, bool> errors = session.ErrorTermIds
            .Distinct()
            .ToDictionary(termId => termId, _ => true);

        _trainingService.SaveSessionResult(result, errors);

        Console.WriteLine();
        Console.WriteLine($"Результат: {result.CorrectAnswers}/{result.TotalQuestions} ({result.SuccessRate:F2}%)");
        Pause();
    }

    private void ShowStatistics()
    {
        ShowHeader("Статистика");
        AppState state = _termManager.State;

        if (state.History.Count == 0)
        {
            Console.WriteLine("История тренировок пока пуста.");
        }
        else
        {
            Console.WriteLine("История тренировок:");
            foreach (SessionResult result in state.History.OrderByDescending(result => result.DateTime))
            {
                Console.WriteLine($"{result.DateTime:g} | {result.ThemeName} | {result.CorrectAnswers}/{result.TotalQuestions} | {result.SuccessRate:F2}%");
            }
        }

        Console.WriteLine();
        Console.WriteLine("Самые сложные термины:");
        List<KeyValuePair<Guid, int>> difficultTerms = state.Statistics.GetMostDifficultTerms(10);

        if (difficultTerms.Count == 0)
        {
            Console.WriteLine("Ошибок пока нет.");
        }
        else
        {
            foreach (KeyValuePair<Guid, int> difficultTerm in difficultTerms)
            {
                Term? term = FindTermById(difficultTerm.Key);
                string termName = term?.Word ?? difficultTerm.Key.ToString();
                Console.WriteLine($"{termName}: ошибок — {difficultTerm.Value}");
            }
        }

        Pause();
    }

    private void CreateSet()
    {
        string name = ReadRequiredText("Название набора: ");
        _termManager.CreateSet(name);
        ShowMessage("Набор создан.");
    }

    private void RenameSet()
    {
        TermSet? termSet = SelectTermSet();
        if (termSet is null)
        {
            return;
        }

        string newName = ReadRequiredText("Новое название: ");
        _termManager.RenameSet(termSet.Id, newName);
        ShowMessage("Набор переименован.");
    }

    private void DeleteSet()
    {
        TermSet? termSet = SelectTermSet();
        if (termSet is null)
        {
            return;
        }

        _termManager.DeleteSet(termSet.Id);
        ShowMessage("Набор удален.");
    }

    private void AddTerm(TermSet termSet)
    {
        string word = ReadRequiredText("Термин: ");
        List<string> definitions = ReadDefinitions();
        _termManager.AddTerm(termSet.Id, word, definitions);
        ShowMessage("Термин добавлен.");
    }

    private void CreateTermFromSearch(string word)
    {
        TermSet? termSet = SelectTermSet();
        if (termSet is null)
        {
            return;
        }

        List<string> definitions = ReadDefinitions();
        _termManager.AddTerm(termSet.Id, word, definitions);
        ShowMessage("Термин создан.");
    }

    private void EditTerm(TermSet termSet)
    {
        Term? term = SelectTerm(termSet);
        if (term is not null)
        {
            EditTerm(termSet, term);
        }
    }

    private void EditTerm(TermSet termSet, Term term)
    {
        string newWord = ReadRequiredText("Новое название термина: ");
        List<string> definitions = ReadDefinitions();
        _termManager.EditTerm(termSet.Id, term.Id, newWord, definitions);
        ShowMessage("Термин обновлен.");
    }

    private void DeleteTerm(TermSet termSet)
    {
        Term? term = SelectTerm(termSet);
        if (term is not null)
        {
            DeleteTerm(termSet, term);
        }
    }

    private void DeleteTerm(TermSet termSet, Term term)
    {
        _termManager.DeleteTerm(termSet.Id, term.Id);
        ShowMessage("Термин удален.");
    }

    private void AddDefinition(TermSet termSet)
    {
        Term? term = SelectTerm(termSet);
        if (term is not null)
        {
            AddDefinition(termSet, term);
        }
    }

    private void AddDefinition(TermSet termSet, Term term)
    {
        string definition = ReadRequiredText("Новый вариант ответа: ");
        _termManager.AddDefinition(termSet.Id, term.Id, definition);
        ShowMessage("Вариант ответа добавлен.");
    }

    private void RemoveDefinition(TermSet termSet)
    {
        Term? term = SelectTerm(termSet);
        if (term is null)
        {
            return;
        }

        if (term.Definitions.Count <= 1)
        {
            ShowMessage("Нельзя удалить последний вариант ответа.");
            return;
        }

        PrintTerm(term);
        int definitionIndex = ReadNumber("Номер варианта ответа для удаления: ", 1, term.Definitions.Count) - 1;
        bool removed = _termManager.RemoveDefinition(termSet.Id, term.Id, term.Definitions[definitionIndex]);
        ShowMessage(removed ? "Вариант ответа удален." : "Не удалось удалить вариант ответа.");
    }

    private TermSet? SelectTermSet()
    {
        if (_termManager.TermSets.Count == 0)
        {
            ShowMessage("Наборов пока нет.");
            return null;
        }

        PrintTermSets();
        Console.WriteLine("0. Назад");
        int selectedNumber = ReadNumber("Выберите набор: ", 0, _termManager.TermSets.Count);

        return selectedNumber == 0 ? null : _termManager.TermSets[selectedNumber - 1];
    }

    private Term? SelectTerm(TermSet termSet)
    {
        if (termSet.Terms.Count == 0)
        {
            ShowMessage("В наборе пока нет терминов.");
            return null;
        }

        PrintTerms(termSet);
        Console.WriteLine("0. Назад");
        int selectedNumber = ReadNumber("Выберите термин: ", 0, termSet.Terms.Count);

        return selectedNumber == 0 ? null : termSet.Terms[selectedNumber - 1];
    }

    private Term? FindTermById(Guid termId)
    {
        return _termManager.TermSets
            .SelectMany(termSet => termSet.Terms)
            .FirstOrDefault(term => term.Id == termId);
    }

    private static List<string> ReadDefinitions()
    {
        List<string> definitions = new();

        while (definitions.Count == 0)
        {
            string definition = ReadRequiredText("Вариант ответа: ");
            definitions.Add(definition);
        }

        while (true)
        {
            string answer = ReadMenuChoice("Добавить еще вариант ответа? (Y/N): ");
            if (answer.Equals("Y", StringComparison.OrdinalIgnoreCase))
            {
                definitions.Add(ReadRequiredText("Вариант ответа: "));
            }
            else if (answer.Equals("N", StringComparison.OrdinalIgnoreCase))
            {
                return definitions;
            }
        }
    }

    private void PrintTerms(TermSet termSet)
    {
        Console.WriteLine("Термины:");
        for (int i = 0; i < termSet.Terms.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {termSet.Terms[i].Word}");
        }
    }

    private void PrintTermSets()
    {
        Console.WriteLine("Список наборов:");
        for (int i = 0; i < _termManager.TermSets.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {_termManager.TermSets[i].Name} ({_termManager.TermSets[i].Terms.Count} терминов)");
        }
    }

    private static void PrintTerm(Term term)
    {
        Console.WriteLine($"Термин: {term.Word}");
        Console.WriteLine("Варианты ответа:");

        for (int i = 0; i < term.Definitions.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {term.Definitions[i]}");
        }
    }

    private static string ReadRequiredText(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string? input = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(input))
            {
                return input.Trim();
            }

            Console.WriteLine("Значение не может быть пустым.");
        }
    }

    private static string ReadMenuChoice(string prompt)
    {
        Console.Write(prompt);
        return Console.ReadLine()?.Trim() ?? string.Empty;
    }

    private static int ReadNumber(string prompt, int minValue, int maxValue)
    {
        while (true)
        {
            Console.Write(prompt);
            string? input = Console.ReadLine();

            if (int.TryParse(input, out int value) && value >= minValue && value <= maxValue)
            {
                return value;
            }

            Console.WriteLine($"Введите число от {minValue} до {maxValue}.");
        }
    }

    private static void ShowHeader(string title)
    {
        Console.Clear();
        Console.WriteLine();
        Console.WriteLine("========================================");
        Console.WriteLine(title);
        Console.WriteLine("========================================");
    }

    private static void ShowMessage(string message)
    {
        Console.WriteLine(message);
        Pause();
    }

    private static void Pause()
    {
        Console.WriteLine("Нажмите Enter для продолжения...");
        Console.ReadLine();
        Console.Clear();
    }
}