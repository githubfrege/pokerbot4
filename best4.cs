using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
namespace pokerbot3
{

    static class Program
    {
        public static ulong Deck = 0b_11111111_1111100_11111111_1111100_11111111_1111100_11111111_1111100;
        public static Dictionary<ulong, (int, long)> _scoreTable = new Dictionary<ulong, (int, long)>();
        public static Dictionary<(ulong, ulong), (int, long)> _handTable = new Dictionary<(ulong, ulong), (int, long)>();

        private static int compareKvp(KeyValuePair<int, int> a, KeyValuePair<int, int> b)
        {
            return a.Key.CompareTo(b.Key);
        }
        public static ulong parseAsBitField(List<KeyValuePair<int, int>> cards)
        {
            ulong bf = 0;
            foreach (var card in cards)
            {
                bf |= 1UL << (card.Key + (15 * card.Value));
            }
            return bf;

        }
        private static bool isStraight(int solo)
        {
            int lsb = solo & -solo;

            int normalized = solo / lsb;

            return normalized == 31;

        }
        private static int getHighestRank(ulong allCards, ref int pos)
        {
            pos = 63 - BitOperations.LeadingZeroCount((ulong)allCards | 1);

            //Console.WriteLine((int)Math.Floor((double)(pos / 4)));
            return (int)Math.Floor((double)(pos / 4));
        }

        private static long getTieBreaker(ulong ranksField)
        {
            int pos = 0;
            int first = getHighestRank(ranksField, ref  pos);
            ranksField ^= (1UL << pos);
            int second = getHighestRank(ranksField, ref pos);
            ranksField ^= (1UL << pos);
            int third = getHighestRank(ranksField, ref pos);
            ranksField ^= (1UL << pos);
            int fourth = getHighestRank(ranksField, ref pos);
            ranksField ^= (1UL << pos);
            int fifth = getHighestRank(ranksField, ref pos);
            int tiebreaker = first << 16 | second << 12 | third << 8 | fourth << 4 | fifth << 0;
            /*for (int i = 0; i < 4; i++)
            {
                first = getHighestRank(ranksField ^ (1UL << first));
                Console.WriteLine(first);
                tiebreaker |= first << 16 - ((i + 1) * 4);

            }*/
            return tiebreaker;
        }
        public static int getMainScore(ulong bf, int solo, ulong ranksField, bool flush)
        {
            bool straight = isStraight(solo);
            if (straight && flush)
            {
                if (solo == 31744)
                {
                    return 10;
                }
                else
                {
                    return 9;
                }

            }
            else
            {
                switch (ranksField % 15)
                {
                    case 1:
                        return 8;
                    case 10:
                        return 7;
                        
                    case 9:
                        return 4;
                    case 7:
                        return 3;
                    case 6:
                        return 2;
                    default:
                        break;
                }
               
               if (flush)
               {
                    return 6;
               }
               else if (straight || solo == 16444)
               {
                    return 5;
               }
                return 1;

            }
        }
        public static (int mainScore, long tieBreaker) getFullScore(ulong bf)
        {
            if (_scoreTable.ContainsKey(bf))
            {
                return _scoreTable[bf];
            }
            getFields(bf, out int solo, out ulong ranksField, out bool flush);

            //Console.WriteLine(Convert.ToString(solo, toBase: 2));
            //Console.WriteLine("is straight:" + isStraight(solo));
            /*int score = 0;
            if (straight && flush)
            {
                if (solo == 31744)
                {
                    str = "royal flush";
                    score = 10;
                }
                else
                {
                   str = "straight flush";

                    score = 9;
                }

            }
            else
            {
                switch (ranksField % 15)
                {
                    case 1:
                       str ="four of a kind";

                        score = 8;
                        break;
                    case 10:
                        str = "full house";

                        score = 7;
                        break;
                    case 9:
                        str = "three of a kind";
                        score = 4;
                        break;
                    case 7:
                        str = "two pairs";

                        score = 3;
                        break;
                    case 6:
                        str = "one pair";
                        score = 2;
                        break;
                    case 5:
                        str = "high card";
                        score = 1;
                        break;
                    default:
                        break;
                }
                if (score < 6)
                {
                    if (flush)
                    {
                        str = "flush";
                        score = 6;
                    }
                    else if (straight || solo == 16444)
                    {
                        str = "straight";

                        score = 5;
                    }
                }
              
            }*/
            //Console.WriteLine(str);
            (int, long) result = (getMainScore(bf,solo,ranksField,flush), getTieBreaker(ranksField));
            //Console.WriteLine(score);
            _scoreTable[bf] = result;
            return result;
        }
        /*public static int getSolo(ulong bf)
        {
            int solo = 0;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 2; j <= 14; j++)
                {
                    if ((bf & (1UL << (j + (15 * i)))) > 0)
                    {
                        solo |= (1 << j);
                    }
                   
                }

            }


            return solo;
        }*/
        public static void getFields(ulong bf, out int solo, out ulong ranksField, out bool flush)
        {
            solo = 0;
            ranksField = 0;
            flush = false;
            Dictionary<int, int> instances = new Dictionary<int, int>();
            int cards = 0;
            for (int i = 0; i < 4; i++)
            {
                int flushIdx = 0;
                for (int j = 2; j <= 14; j++)
                {
                    
                    
                    if ((bf &(1UL << (j+ (15 * i)))) > 0)            //if ((bf & (1UL << (j + (15 * i)))) != 0)
                    {
                        cards++;
                        solo |= (1 << j);
                        flushIdx++;
                        if (flushIdx == 5)
                        {
                            flush = true;
                        }
                        if (!instances.ContainsKey(j))
                        {
                            instances.Add(j, 0);
                        }
                        else
                        {
                            instances[j] = instances[j] + 1;
                        }

                        int offset = instances[j];
                        ulong addition = 1UL << (j << 2);
                        addition = addition << offset;
                        ranksField |= addition;

                    }

                }
            }
            //Console.WriteLine(Convert.ToString(solo, toBase: 2));


        }
        public static IEnumerable<ulong> ToIEnum(this ulong num)
        {
            for (int i = 2; i <= 60; i++)
            {
                if ((num & (1UL << i)) > 0)
                {
                    yield return 1UL << i;
                }
            }
        }
        private static IEnumerable<ulong> cardCombos(IEnumerable<ulong> cards, int count)
        {
            int i = 0;
            foreach (var card in cards)
            {
                if (count == 1)
                {
                    yield return card;
                }

                else
                {
                    foreach (var result in cardCombos(cards.Skip(i + 1), count - 1))
                    {

                        yield return result | card;
                    }
                }

                ++i;
            }
        }
        public static int WhatToBet(float odds, int tableChips, int chipOffer, int myChips)
        {
            int bet = 0;
            for (int i = chipOffer; i < myChips + chipOffer; i++)
            {
                if (((odds * (i + tableChips)) + ((100 - odds) * i)) > 0)
                {
                    bet = i;
                }
            }
            return bet;
        }
        public static float GetOdds(List<KeyValuePair<int, int>> holeCardsKvpList, List<KeyValuePair<int, int>> tableKvpList)
        {
            float heroWins = 0; //if player wins
            float villainWins = 0; //if opponent wins
            float villainCardSets = 0;
            ulong holes = parseAsBitField(holeCardsKvpList);
            ulong communityCards = parseAsBitField(tableKvpList);
            ulong availableCards = Deck ^ (holes | communityCards);
            foreach (ulong villainHoles in cardCombos(availableCards.ToIEnum(), 2)) //every combination of cards our opponent may have
            {
                villainCardSets++;
                Console.WriteLine(villainCardSets);

                ulong currentAvailableCards = availableCards ^ villainHoles;
                foreach (ulong cardAdditions in cardCombos(currentAvailableCards.ToIEnum(), 5 - tableKvpList.Count)) //all combinations of cards that may be added to the existing table
                {
                    ulong currentHand = communityCards | cardAdditions;
                    (float heroPoints, float villainPoints) scoreUpdate = updateScore(holes, villainHoles, currentHand);
                    heroWins += scoreUpdate.heroPoints;
                    villainWins += scoreUpdate.villainPoints;

                }

            }
            return heroWins / (heroWins + villainWins);

        }
        private static (float heroPoints, float villainPoints) updateScore(ulong holes, ulong villainHoles, ulong currentHand)
        {
            (int mainScore, long tieBreaker) heroHand = handToPlay(holes, currentHand);
            (int mainScore, long tieBreaker) villainHand = handToPlay(villainHoles, currentHand);
            switch (heroHand.mainScore.CompareTo(villainHand.mainScore) != 0 ? heroHand.mainScore.CompareTo(villainHand.mainScore) : heroHand.tieBreaker.CompareTo(villainHand.tieBreaker))
            {
                case 1:
                    return (1, 0);
                case -1:
                    return (0, 1);
                default:
                    return (0.5f, 0.5f);
            }

        }
        private static (int mainScore, long tieBreaker) handToPlay(ulong holes, ulong cardsOnTable)
        {
            if (_handTable.ContainsKey((holes, cardsOnTable)))
            {
                return _handTable[(holes, cardsOnTable)];
            }

            (int mainScore, long tieBreaker) max = (-100000, -100000);
            foreach (ulong combo in cardCombos(cardsOnTable.ToIEnum(), 3))
            {
                (int mainScore, long tieBreaker) currentScore = getFullScore(combo | holes);
                if ((currentScore.mainScore.CompareTo(max.mainScore) != 0 ? currentScore.mainScore.CompareTo(max.mainScore) : currentScore.tieBreaker.CompareTo(max.tieBreaker)) > 0)
                {
                    max = currentScore;
                }

            }
            foreach (ulong combo in cardCombos(cardsOnTable.ToIEnum(), 4))
            {
                foreach (ulong holeCard in holes.ToIEnum())
                {
                    (int mainScore, long tieBreaker) currentScore = getFullScore(combo | holeCard);
                    if ((currentScore.mainScore.CompareTo(max.mainScore) != 0 ? currentScore.mainScore.CompareTo(max.mainScore) : currentScore.tieBreaker.CompareTo(max.tieBreaker)) > 0)
                    {
                        max = currentScore;
                    }

                }
            }
            _handTable[(holes, cardsOnTable)] = max;
            return max;
        }
        /*private static IEnumerable<ulong> cardCombos(ulong cards, int count, int discarded)
        {
            for (int idx = discarded; idx <= 60; idx++)
            {
                ulong card = cards & (1UL << idx);
                if (card > 0)
                {
                    if (count == 1)
                    {
                        yield return card;
                    }
                    foreach (ulong result in cardCombos(cards ^ card, count - 1, idx))
                    {
                        yield return card |= result;
                    }
                }
            }
        }*/

        /*public static long getRanksField(ulong bf)
        {
            long ranksField = 0;
            Dictionary<int, int> instances = new Dictionary<int, int>();
            for (int i = 0; i < 3; i++)
            {
                for (int j = 2; j <= 14; j++)
                {
                    int offset = 0;
                    if ((bf & (1UL << (j + (16 * i)))) != 0)
                    {

                        if (!instances.ContainsKey(j))
                        {
                            instances.Add(j, 1);
                        }
                        else
                        {
                            instances[j] = instances[j] + 1;
                        }
                        offset = instances[j];

                    }
                    long addition = 1 << (j << 2);
                    addition = addition << offset;
                    ranksField |= addition;
                        
                }
                
            }
        }*/
        static void Main(string[] args)
        {
            int[] holeCardSuits = new int[] { 3, 1 };
            int[] holeCardRanks = new int[] { 13, 3 };
            int[] tableCardSuits = new int[] { 1, 3,0 };
            int[] tableCardRanks = new int[] { 2,9,11};
            List<KeyValuePair<int, int>> myHoleCards = new List<KeyValuePair<int, int>>();
            List<KeyValuePair<int, int>> tableCards = new List<KeyValuePair<int, int>>();

            for (int i = 0; i < 2; i++)
            {
                myHoleCards.Add(new KeyValuePair<int, int>(holeCardRanks[i], holeCardSuits[i]));
            }

            for (int i = 0; i < 3; i++)
            {
                tableCards.Add(new KeyValuePair<int, int>(tableCardRanks[i], tableCardSuits[i]));
            }

            int[] holeCardSuits2 = new int[] { 2, 3 };
            int[] holeCardRanks2 = new int[] { 7, 14 };
            int[] tableCardSuits2 = new int[] { 1,1,1,1,1};
            int[] tableCardRanks2 = new int[] {2,3,4,5,6};
            List<KeyValuePair<int, int>> myHoleCards2 = new List<KeyValuePair<int, int>>();
            List<KeyValuePair<int, int>> tableCards2 = new List<KeyValuePair<int, int>>();
            for (int i = 0; i < 2; i++)
            {
                myHoleCards2.Add(new KeyValuePair<int, int>(holeCardRanks2[i], holeCardSuits2[i]));
            }

            for (int i = 0; i < 5; i++)
            {
                tableCards2.Add(new KeyValuePair<int, int>(tableCardRanks2[i], tableCardSuits2[i]));
            }
            Console.WriteLine(GetOdds(myHoleCards, tableCards));


            /*getFields(bf, out int solo, out ulong ranksField, out bool flush);
            Console.WriteLine(Convert.ToString((long)bf, toBase: 2));
            //ulong newBitMask = (1UL << (13 + (15 * 3))) | (1UL << 2) | (1UL << 3) | (1UL << 14) |  (1UL << 6);
            //Console.WriteLine(Convert.ToString((long)newBitMask, toBase: 2));
            Console.WriteLine(Convert.ToString((long)ranksField, toBase: 2));
            Console.WriteLine(Convert.ToString(solo, toBase: 2));*/
            ;
            //Console.WriteLine("new");*/
            
         
        }
    }
}
