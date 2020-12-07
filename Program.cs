using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Third
{
    internal class Program
    {
        private const string InvalidMoveMesssage = "Invalid move key. Move key must be" +
            " integer number from the available moves. Try again.";

        private const string ExecExceptionMessage = "Invalid input parameters. \n" +
            "Parameters must be uniq strings.Their count must be odd and equals at least 3. \n" +
            "Exampple: thrid.exe rock paper scissors lizard Spock";

        private static readonly Dictionary<int, string> _movesDictionary = new Dictionary<int, string>();

        private static void Main(string[] args)
        {
            if (!InputsAreValid(args))
            {
                Console.WriteLine(ExecExceptionMessage);
                return;
            }

            InitializeMovesDictionary(args);

            var aiMove = GetAIMove();

            Console.WriteLine($"HMAC: {aiMove.HMACStringView}");
            Console.WriteLine("Available moves:");

            foreach (var mov in _movesDictionary)
                Console.WriteLine($"{mov.Key} : {mov.Value}");

            int plrMoveKey = GetPlayerMove();
            if (plrMoveKey == 0)
                return;

            Console.WriteLine($"Your move: {_movesDictionary[plrMoveKey]}");
            Console.WriteLine($"Computer move: {_movesDictionary[aiMove.MoveKey]}");

            int res = CalcResult(plrMoveKey, aiMove.MoveKey);
            switch (res)
            {
                case 0:
                    Console.WriteLine("Draw!");
                    break;

                case 1:
                    Console.WriteLine("You win!");
                    break;

                case 2:
                    Console.WriteLine("You lose!");
                    break;
            }

            Console.WriteLine($"HMAC key: {aiMove.HMACKeyStringView}");
        }

        private static bool InputsAreValid(string[] args)
        {
            if (args.Length < 3 || args.Length % 2 == 0)
                return false;

            for (int i = 0; i < args.Length; i++)
            {
                for (int j = i + 1; j < args.Length; j++)
                {
                    if (args[i] == args[j])
                        return false;
                }
            }

            return true;
        }

        private static void InitializeMovesDictionary(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                _movesDictionary.Add(i + 1, args[i]);
            }
            _movesDictionary.Add(0, "exit");
        }

        private static EncryptedMove GetAIMove()
        {
            Random rnd = new Random();
            int moveKey = rnd.Next(1, _movesDictionary.Count - 1);

            return new EncryptedMove(moveKey, _movesDictionary);
        }

        private static int GetPlayerMove()
        {
            while (true)
            {
                if (int.TryParse(Console.ReadLine(), out int moveKey) && _movesDictionary.ContainsKey(moveKey))
                    return moveKey;
                else
                    Console.WriteLine(InvalidMoveMesssage);
            }
        }

        private static int CalcResult(int playersMoveKey, int aiMoveKey)
        {
            if (playersMoveKey == aiMoveKey)
                return 0;

            float diff = aiMoveKey - playersMoveKey;
            float offset = (_movesDictionary.Count - 1) / 2.0f;

            if ((diff > 0 && diff > offset) || (diff < 0 && -diff < offset))
                return 1;
            else
                return 2;
        }

        private class EncryptedMove
        {
            internal readonly int MoveKey;
            internal readonly byte[] HMACKey;
            internal readonly byte[] HMAC;

            internal string HMACStringView
            {
                get
                {
                    return BitConverter.ToString(HMAC).Replace("-", "");
                }
            }

            internal string HMACKeyStringView
            {
                get
                {
                    return BitConverter.ToString(HMACKey).Replace("-", "");
                }
            }

            internal EncryptedMove(int moveKey, Dictionary<int, string> movesDictionary)
            {
                MoveKey = moveKey;
                HMACKey = new byte[16];

                using (var cRnd = new RNGCryptoServiceProvider())
                    cRnd.GetBytes(HMACKey);

                var hmac = new HMACSHA256(HMACKey);
                string aiMove = movesDictionary[moveKey];
                HMAC = hmac.ComputeHash(Encoding.ASCII.GetBytes(aiMove));
            }
        }
    }
}