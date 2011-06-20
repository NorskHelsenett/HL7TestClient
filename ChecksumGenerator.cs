using System.Linq;

namespace HL7TestClient
{
    public class ChecksumGenerator
    {
        public static bool AppendChecksum(ref string number)
        {
            var e = number.Select(c => c - '0').ToArray();
            var k1 = 11 - ((3 * e[0] + 7 * e[1] + 6 * e[2] + 1 * e[3] + 8 * e[4] + 9 * e[5] + 4 * e[6] + 5 * e[7] + 2 * e[8]) % 11);
            k1 = (11 == k1) ? 0 : k1;
            var k2 = 11 - ((5 * e[0] + 4 * e[1] + 3 * e[2] + 2 * e[3] + 7 * e[4] + 6 * e[5] + 5 * e[6] + 4 * e[7] + 3 * e[8] + 2 * k1) % 11);
            k2 = (11 == k2) ? 0 : k2;
            if (k1 == 10 || k2 == 10)
                return false;
            number = number + k1 + k2;
            return true;
        }
    }
}
