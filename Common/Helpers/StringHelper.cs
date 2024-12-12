using System.Text;

namespace Common.Helpers
{
    public class StringHelper
    {
        private const string NUMBERS = "0123456789";
        private const string CHARACTERS = "abcdefghijklmnopqrstuvwxyz";
        private const string SYMBOLS = "!@#$%^*&~";

        public static string RandomString(int length,
                                          bool includeCharacters = true,
                                          bool includeNumber = true,
                                          bool includeSymbols = true)
        {
            var collection = new List<String>();
            var stringBuilder = new StringBuilder();
            var random = new Random();

            if (includeCharacters)
                collection.Add(CHARACTERS);

            if (includeNumber)
                collection.Add(NUMBERS);

            if (includeSymbols)
                collection.Add(SYMBOLS);

            while (stringBuilder.Length < length)
            {
                var index = random.Next(0, collection.Count);
                var choice = random.Next(0, collection[index].Length);
                stringBuilder.Append(collection[index][choice]);
            }

            return stringBuilder.ToString();
        }
    }
}
