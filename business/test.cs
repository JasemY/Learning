using System;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using System.IO;
using System.Configuration;
using System.Text;
using System.Text.RegularExpressions;
using System.Data;
using System.Data.SqlClient;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Data.Entity;

namespace Ia.Learning.Cl.Model.Business
{
    ////////////////////////////////////////////////////////////////////////////

    /// <summary publish="true">
    /// Test business support functions
    /// </summary>
    /// <value>
    /// https://msdn.microsoft.com/en-us/library/z1hkazw7(v=vs.100).aspx
    /// </value>
    /// <remarks> 
    /// Copyright © 2008-2015 Jasem Y. Al-Shamlan (info@ia.com.kw), Internet Applications - Kuwait. All Rights Reserved.
    ///
    /// This library is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
    /// the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
    ///
    /// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
    /// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
    /// 
    /// You should have received a copy of the GNU General Public License along with this library. If not, see http://www.gnu.org/licenses.
    /// 
    /// Copyright notice: This notice may not be removed or altered from any source distribution.
    /// </remarks> 
    public partial class Test
    {
        static Random random = new Random(DateTime.UtcNow.Second);
        //static int numberOfCumulativeAnswerCorrectnessIndicatorThatIndicatesRetention = 7;
        //static int numberOfPoolWords = 7 * 7;
        static int numberOfResponseSecondsThatIndicateMemorization = 3;

        public enum TestTopic { GermanWords = 1, Multiplication = 2, Kanji = 3 };
        public enum AnswerType { Correct, CorrectButSlow, Incorrect }

        /*
         * Calculate and return the proper words withing a proper question range based on the test taking history from the database
         * 
         * - Initially all words will have Enabled = false.
         * - The process will assign a pool of enabled words for testing.
         * - At any time the user will be memorizing the pool of words only.
         * - If a word is consedered memorized by its test history it will exit the word testing pool and another will be randomly added
         * - A word will not be tested again until at least a minute passes.
         */

        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        ///
        /// </summary>
        public Test() { }

        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        ///
        /// </summary>
        public static void CalculatedTestQuestionAndOptions(Ia.Learning.Cl.Model.Business.Test.TestTopic test, out Ia.Learning.Cl.Model.Score testQuestion, out List<Ia.Learning.Cl.Model.Score> optionList, out string debug)
        {
            int testQuestionTypeId, testQuestionId, optionIndex1, optionIndex2, optionIndex3;
            DateTime threeMinutesAgo;
            List<Ia.Learning.Cl.Model.Score> questionList;

            debug = "";
            optionIndex1 = optionIndex2 = optionIndex3 = 0;
            optionList = new List<Ia.Learning.Cl.Model.Score>();

            threeMinutesAgo = DateTime.UtcNow.AddHours(3).AddMinutes(-3);

            // below:
            testQuestionTypeId = ReturnRandomTypeIdAccordingTypeDistribution(test);

            using (var db = new Ia.Learning.Cl.Model.Learning())
            {
                testQuestion = (from q in db.Scores
                                where q.TestId == (int)test && q.TypeId == testQuestionTypeId
                                && q.Viewed < threeMinutesAgo
                                orderby q.CumulativeAnswerCorrectnessIndicator, q.NumberOfTimesAsked, q.NumberOfConsecutiveCorrects, Guid.NewGuid() ascending
                                select q).Take(5).FirstOrDefault();

                if (testQuestion != null)
                {
                    testQuestionId = testQuestion.Id;
                    testQuestionTypeId = testQuestion.TypeId;

                    debug = "cu: " + testQuestion.CumulativeAnswerCorrectnessIndicator + "; num ask: " + testQuestion.NumberOfTimesAsked + "; num con cu: " + testQuestion.NumberOfConsecutiveCorrects + "; av cu:" + string.Format("{0:0.00}", Ia.Learning.Cl.Model.Data.Test.AverageCumulativeAnswerCorrectnessIndicator(test));

                    // below: update Viewed
                    testQuestion.Viewed = DateTime.UtcNow.AddHours(3);
                    db.SaveChanges();

                    //debug += "Test:" + list.Count.ToString();

                    questionList = (from q in db.Scores where q.TestId == (int)test && q.TypeId == testQuestionTypeId && q.Id != testQuestionId select q).ToList();

                    if (questionList.Count > 0)
                    {
                        // below: select random, non-similar numbers
                        while (optionIndex1 == optionIndex2 || optionIndex1 == optionIndex3 || optionIndex2 == optionIndex3 || questionList[optionIndex1].Answer == testQuestion.Answer || questionList[optionIndex2].Answer == testQuestion.Answer || questionList[optionIndex3].Answer == testQuestion.Answer)
                        {
                            optionIndex1 = random.Next(questionList.Count);
                            optionIndex2 = random.Next(questionList.Count);
                            optionIndex3 = random.Next(questionList.Count);
                        }

                        try
                        {
                            optionList.Add(questionList[optionIndex1]);
                            optionList.Add(questionList[optionIndex2]);
                            optionList.Add(questionList[optionIndex3]);
                        }
                        catch { }

                        // below: insert the correct answer randomally
                        optionList.Insert(random.Next(optionList.Count + 1), testQuestion);

                        /*
            // below: select random, close, non-similar numbers
            while (optionIndex1 == optionIndex2 || optionIndex1 == optionIndex3 || optionIndex2 == optionIndex3)
            {
                while (
                    (int.Parse(questionList[optionIndex1].Answer) == int.Parse(questionList[optionIndex2].Answer)) ||
                    (int.Parse(questionList[optionIndex1].Answer) == int.Parse(questionList[optionIndex3].Answer)) ||
                    (int.Parse(questionList[optionIndex2].Answer) == int.Parse(questionList[optionIndex3].Answer)) ||

                    (System.Math.Abs(int.Parse(questionList[optionIndex1].Answer) - int.Parse(testQuestion.Answer)) > 10) ||
                    (System.Math.Abs(int.Parse(questionList[optionIndex2].Answer) - int.Parse(testQuestion.Answer)) > 10) ||
                    (System.Math.Abs(int.Parse(questionList[optionIndex3].Answer) - int.Parse(testQuestion.Answer)) > 10) ||

                    (int.Parse(questionList[optionIndex1].Answer) == int.Parse(testQuestion.Answer)) ||
                    (int.Parse(questionList[optionIndex2].Answer) == int.Parse(testQuestion.Answer)) ||
                    (int.Parse(questionList[optionIndex3].Answer) == int.Parse(testQuestion.Answer))
                    )
                {
                    optionIndex1 = random.Next(questionList.Count);
                    optionIndex2 = random.Next(questionList.Count);
                    optionIndex3 = random.Next(questionList.Count);
                }
            }

         */
                    }
                    else
                    {
                    }
                }
                else debug = "Error: testQuestion == null: ";
            }
        }

        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        ///
        /// </summary>
        protected static int ReturnRandomTypeIdAccordingTypeDistribution(Ia.Learning.Cl.Model.Business.Test.TestTopic test)
        {
            int countOfAllTypes, countOfTypeId1, countOfTypeId2, countOfTypeId3, countOfTypeId4, countOfTypeId5, typeId;
            ArrayList countArrayList;

            using (var db = new Ia.Learning.Cl.Model.Learning())
            {
                countOfAllTypes = (from q in db.Scores where q.TestId == (int)test select q).Count();
                countOfTypeId1 = (from q in db.Scores where q.TestId == (int)test && q.TypeId == 1 select q).Count();
                countOfTypeId2 = (from q in db.Scores where q.TestId == (int)test && q.TypeId == 2 select q).Count();
                countOfTypeId3 = (from q in db.Scores where q.TestId == (int)test && q.TypeId == 3 select q).Count();
                countOfTypeId4 = (from q in db.Scores where q.TestId == (int)test && q.TypeId == 4 select q).Count();
                countOfTypeId5 = (from q in db.Scores where q.TestId == (int)test && q.TypeId == 5 select q).Count();

                countArrayList = new ArrayList(countOfAllTypes);

                // below: populate ArrayList with TypeId keys a number of times equal to its count
                for (int i = 0; i < countOfTypeId1; i++) countArrayList.Add(1);
                for (int i = 0; i < countOfTypeId2; i++) countArrayList.Add(2);
                for (int i = 0; i < countOfTypeId3; i++) countArrayList.Add(3);
                for (int i = 0; i < countOfTypeId4; i++) countArrayList.Add(4);
                for (int i = 0; i < countOfTypeId5; i++) countArrayList.Add(5);

                typeId = (int)countArrayList[random.Next(countOfAllTypes)];
            }

            return typeId;
        }

        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        ///
        /// </summary>
        public static Ia.Learning.Cl.Model.Business.Test.AnswerType CheckSelectedAnswerToQuestion(Ia.Learning.Cl.Model.Score testQuestion, Ia.Learning.Cl.Model.Score selectedOption, TimeSpan responseTimeSpan)
        {
            // below: check answers
            Ia.Learning.Cl.Model.Business.Test.AnswerType answerType;
            TimeSpan responseSecondsThatIndicateMemorizationTimeSpan;

            using (var db = new Ia.Learning.Cl.Model.Learning())
            {
                testQuestion = db.Scores.FirstOrDefault(q => q.Id == testQuestion.Id);

                if (testQuestion.Answer == selectedOption.Answer)
                {
                    responseSecondsThatIndicateMemorizationTimeSpan = TimeSpan.Parse("00:00:" + numberOfResponseSecondsThatIndicateMemorization.ToString().PadLeft(2, '0'));

                    if (responseTimeSpan < responseSecondsThatIndicateMemorizationTimeSpan)
                    {
                        testQuestion.CumulativeAnswerCorrectnessIndicator++;
                        testQuestion.NumberOfConsecutiveCorrects++;

                        answerType = Ia.Learning.Cl.Model.Business.Test.AnswerType.Correct;
                    }
                    else
                    {
                        answerType = Ia.Learning.Cl.Model.Business.Test.AnswerType.CorrectButSlow;
                    }
                }
                else
                {
                    testQuestion.CumulativeAnswerCorrectnessIndicator--;
                    testQuestion.NumberOfConsecutiveCorrects = 0;
                    answerType = Ia.Learning.Cl.Model.Business.Test.AnswerType.Incorrect;
                }

                testQuestion.Updated = testQuestion.Viewed = DateTime.UtcNow.AddHours(3);
                testQuestion.NumberOfTimesAsked++;

                db.SaveChanges();
            }

            return answerType;
        }

        ////////////////////////////////////////////////////////////////////////////    
        ////////////////////////////////////////////////////////////////////////////    
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////   
}
