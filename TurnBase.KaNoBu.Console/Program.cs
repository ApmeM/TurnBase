using TurnBase;

namespace TurnBase.KaNoBu;

public class Program
{
    private const int DefaultServerPort = RemoteProtocol.DefaultPort;

    public static async Task Main(string[] args)
    {
        var parameters = new KaNoBuPlayerFactory.GameParameters
        {
            IsClient = false,
            GameId = "test",
            ServerAddress = null,
            FieldSize = 8,
            WithDocks = false,
            EnemyVisible = false,
            MaxMovesPerTurn = 3,
            PlayerTypes = new List<KaNoBuPlayerType>()
        };

        Console.WriteLine("Режим игры: 1 — клиент, 2 — сервер/локальная игра");
        parameters.IsClient = ReadChoice("Выберите режим", new[] { "1", "2" }) == "1";

        if (parameters.IsClient)
        {
            parameters.ServerAddress = ReadNonEmpty("IP-адрес сервера (можно указать IP:порт)");
            var kaNoBuPlayerType = ReadPlayerType("Тип игрока клиента");

            parameters.PlayerTypes.Add(kaNoBuPlayerType);
        }
        else
        {
            parameters.FieldSize = ReadInt("Размер поля (8-20)", 8, 20, 8);
            parameters.WithDocks = ReadYesNo("Добавить доки/стены? (д/н)");
            parameters.EnemyVisible = ReadYesNo("Показывать типы всех кораблей? (д/н)");
            parameters.MaxMovesPerTurn = ReadInt("Максимум ходов за ход (1-20)", 1, 20, 3);
            var playerCount = ReadInt("Количество игроков (2-4)", 2, 4, 2);
            while (parameters.PlayerTypes.Count < playerCount)
            {
                var playerType = ReadPlayerType($"Игрок {parameters.PlayerTypes.Count + 1}");
                if (playerType == KaNoBuPlayerType.Human && parameters.PlayerTypes.Contains(KaNoBuPlayerType.Human))
                {
                    Console.WriteLine("В одной игре может быть только один игрок с управлением из консоли.");
                    continue;
                }

                parameters.PlayerTypes.Add(playerType);
            }
        }

        using var networkServer = new ConsoleNetworkServer();
        var game = KaNoBuPlayerFactory.CreateGame(
            parameters,
            new KaNoBuPlayerConsole(),
            new ConsoleClient(),
            new ConsoleLogger(),
            networkServer,
            Task.Delay);

        if (game == null)
        {
            Console.WriteLine("Не удалось создать игру.");
            return;
        }

        using var cancellation = new CancellationTokenSource();
        ConsoleCancelEventHandler cancelHandler = (_, eventArgs) =>
        {
            eventArgs.Cancel = true;
            cancellation.Cancel();
        };

        Console.CancelKeyPress += cancelHandler;
        await game.Play(cancellation.Token);
        Console.CancelKeyPress -= cancelHandler;
    }




    private static KaNoBuPlayerType ReadPlayerType(string prompt)
    {
        Console.WriteLine($"{prompt}: 1 — изи, 2 — медиум, 3 — ремоут, 4 — консоль");
        return ReadChoice("Выберите тип", new[] { "1", "2", "3", "4" }) switch
        {
            "1" => KaNoBuPlayerType.Easy,
            "2" => KaNoBuPlayerType.Medium,
            "3" => KaNoBuPlayerType.Remote,
            "4" => KaNoBuPlayerType.Human,
            _ => throw new InvalidOperationException()
        };
    }

    private static string ReadChoice(string prompt, IReadOnlyCollection<string> choices)
    {
        while (true)
        {
            Console.Write($"{prompt} [{string.Join('/', choices)}]: ");
            var input = Console.ReadLine()?.Trim();
            if (input != null && choices.Contains(input))
            {
                return input;
            }

            Console.WriteLine("Некорректный выбор, попробуйте ещё раз.");
        }
    }

    private static string ReadNonEmpty(string prompt)
    {
        while (true)
        {
            Console.Write($"{prompt}: ");
            var input = Console.ReadLine()?.Trim();
            if (!string.IsNullOrWhiteSpace(input))
            {
                return input;
            }

            Console.WriteLine("Значение не может быть пустым.");
        }
    }

    private static int ReadInt(string prompt, int min, int max, int defaultValue)
    {
        while (true)
        {
            Console.Write($"{prompt} [{defaultValue}]: ");
            var input = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(input))
            {
                return defaultValue;
            }

            if (int.TryParse(input, out var value) && value >= min && value <= max)
            {
                return value;
            }

            Console.WriteLine($"Введите целое число от {min} до {max}.");
        }
    }

    private static bool ReadYesNo(string prompt)
    {
        return ReadChoice(prompt, new[] { "д", "н", "да", "нет", "y", "n" }) is "д" or "да" or "y";
    }
}