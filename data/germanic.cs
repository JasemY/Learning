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
using System.Reflection;
using System.Collections.Generic;
using System.Data.Entity;

namespace Ia.Learning.Cl.Model.Data
{
    ////////////////////////////////////////////////////////////////////////////

    /// <summary publish="true">
    /// Germanic data support functions
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
    public partial class Germanic
    {
        private static XDocument xDocument;

        public enum WordType { Verb = 1, Noun, Adjective, Adverb, Unspecified };

        /// <summary/>
        public struct Word
        {
            public WordType Type;
            public string English;
            public string German;

            /// <summary/>
            public Word(string english, string german, WordType type)
            {
                this.Type = type;
                this.English = english;
                this.German = german;
            }

            /// <summary/>
            public override string ToString()
            {
                return this.German +":"+ this.English +":"+ this.Type;
            }
        }

        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        ///
        /// </summary>
        public Germanic() { }

        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Read all tests
        /// </summary>
        public static List<Word> ReadList()
        {
            WordType wordType;
            Word word;
            List<Word> wordList;

            wordList = new List<Word>(10000);

            foreach (XElement xe in XDocument.Elements("germanic").Elements())
            {
                switch (xe.Name.LocalName)
                {
                    case "verb": wordType = WordType.Verb; break;
                    case "noun": wordType = WordType.Noun; break;
                    case "adjective": wordType = WordType.Adjective; break;
                    case "adverb": wordType = WordType.Adverb; break;
                    default: wordType = WordType.Unspecified; break;
                }

                word = new Word(xe.Attribute("english").Value, xe.Attribute("german").Value, wordType);

                wordList.Add(word);
            }

            return wordList;
        }

        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 
        /// </summary>        
        [Obsolete("", true)]
        public static void CheckAndMergeGermanXmlEntries()
        {
            StringBuilder sb;
            Word final;
            List<Word> firstList, middleList, tempList, lastList;

            firstList = ReadList();
            middleList = ReadList();

            sb = new StringBuilder(firstList.Count * 100);

            tempList = new List<Word>(10);


            // below: examine and handle exact (English, German, and type) redundant entries

            lastList = new List<Word>(firstList.Count);

            foreach (Word word in firstList)
            {
                if (!lastList.Contains(word))
                {
                    tempList = (from q in middleList where q.German == word.German && q.English == word.English && q.Type == word.Type select q).ToList<Word>();

                    if (tempList.Count > 0)
                    {
                        final = new Word();

                        final.German = word.German;
                        final.English = word.English;
                        final.Type = word.Type;

                        lastList.Add(final);

                        middleList.Remove(final);
                    }
                    else
                    {
                    }
                }
            }


            // below: examine and handle redundant (English, German) entries but keep the one with a word type

            firstList = lastList;
            middleList.Clear();
            lastList.ForEach(i => middleList.Add(i));

            lastList = new List<Word>(firstList.Count);

            foreach (Word word in firstList)
            {
                if (!lastList.Contains(word))
                {
                    tempList = (from q in middleList where q.German == word.German && q.English == word.English select q).ToList<Word>();

                    if (tempList.Count == 1 || tempList.Count == 2)
                    {
                        final = new Word();

                        final.German = word.German;
                        final.English = word.English;

                        if (tempList.Count == 1)
                        {
                            final.Type = word.Type;
                            lastList.Add(final);
                            middleList.Remove(final);
                        }
                        else if (tempList.Count == 2)
                        {
                            if (tempList[0].Type != WordType.Unspecified && tempList[1].Type == WordType.Unspecified)
                            {
                                final.Type = tempList[0].Type;
                                lastList.Add(final);

                                middleList.Remove(tempList[0]);
                                middleList.Remove(tempList[1]);
                            }
                            else if (tempList[0].Type == WordType.Unspecified && tempList[1].Type != WordType.Unspecified)
                            {
                                final.Type = tempList[1].Type;
                                lastList.Add(final);

                                middleList.Remove(tempList[0]);
                                middleList.Remove(tempList[1]);
                            }
                            else
                            {
                                // below: a word is both adverb and adjective
                                final.Type = tempList[0].Type;
                                lastList.Add(final);
                                middleList.Remove(final);

                                final.Type = tempList[1].Type;
                                lastList.Add(final);
                                middleList.Remove(final);
                            }
                        }
                    }
                    else
                    {
                        // below: manually fix XML
                    }
                }
            }


            // below: remove exact redundant entries (German only)

            firstList = lastList;
            middleList.Clear();
            lastList.ForEach(i => middleList.Add(i));

            lastList = new List<Word>(firstList.Count);

            foreach (Word word in firstList)
            {
                if (!lastList.Contains(word))
                {
                    tempList = (from q in middleList where q.German == word.German select q).ToList<Word>();

                    if (tempList.Count > 0)
                    {
                        if (tempList.Count == 1 || tempList.Count == 2)
                        {
                            final = new Word();

                            final.German = word.German;

                            if (tempList.Count == 1)
                            {
                                final.Type = word.Type;
                                final.English = word.English;
                                lastList.Add(final);
                                middleList.Remove(final);
                            }
                            else if (tempList.Count == 2)
                            {
                                if (tempList[0].English.Contains(tempList[1].English))
                                {
                                    final.Type = tempList[0].Type;
                                    final.English = tempList[0].English;
                                    lastList.Add(final);

                                    middleList.Remove(tempList[0]);
                                    middleList.Remove(tempList[1]);
                                }
                                else if (tempList[1].English.Contains(tempList[0].English))
                                {
                                    final.Type = tempList[1].Type;
                                    final.English = tempList[1].English;
                                    lastList.Add(final);

                                    middleList.Remove(tempList[0]);
                                    middleList.Remove(tempList[1]);
                                }
                                else
                                {
                                    final.Type = tempList[0].Type;
                                    final.English = tempList[0].English;
                                    lastList.Add(final);
                                    middleList.Remove(tempList[0]);

                                    final.Type = tempList[1].Type;
                                    final.English = tempList[1].English;
                                    lastList.Add(final);
                                    middleList.Remove(tempList[1]);

                                    //sb.AppendLine(tempList[0].ToString());
                                    //sb.AppendLine(tempList[1].ToString());
                                }
                            }
                        }
                        else
                        {
                            // below: manually fix XML

                            sb.AppendLine(word.ToString());
                        }
                    }
                }
            }

            // below: create new XML file
            foreach(Word word in lastList)
            {
                sb.AppendLine(@"<" + word.Type.ToString().ToLower() + @" german=""" + word.German + @""" english=""" + word.English + @"""/>");
            }

        }

        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        ///
        /// </summary>
        public static void UpdateTestDatabaseWithGermanicAndSynchronizeWithXmlFiles(string identityUserId)
        {
            string question, answer;
            XDocument xDocument;
            WordType wordType;
            Ia.Learning.Cl.Model.Score score;
            List<Ia.Learning.Cl.Model.Score> scoreList;

            xDocument = Ia.Learning.Cl.Model.Data.Germanic.XDocument;

            // below: synch database words with XML list
            using (var db = new Ia.Learning.Cl.Model.Learning())
            {
                scoreList = Ia.Learning.Cl.Model.Score.GermanWordScoreList(identityUserId);

                foreach (XElement xe in xDocument.Elements("germanic").Elements())
                {
                    switch(xe.Name.LocalName)
                    {
                        case "verb": wordType = WordType.Verb; break;
                        case "noun": wordType = WordType.Noun; break;
                        case "adjective": wordType = WordType.Adjective; break;
                        case "adverb": wordType = WordType.Adverb; break;
                        default: wordType = WordType.Unspecified; break;
                    }

                    question = xe.Attribute("german").Value;
                    answer = xe.Attribute("english").Value;

                    score = (from q in scoreList where q.TestId == (int)Ia.Learning.Cl.Model.Business.Test.TestTopic.GermanWords && q.Question == question && q.Answer == answer && q.TypeId == (int)wordType select q).FirstOrDefault();

                    if (score == null)
                    {
                        score = new Ia.Learning.Cl.Model.Score();

                        score.Question = question;
                        score.Answer = answer;
                        score.TestId = (int)Ia.Learning.Cl.Model.Business.Test.TestTopic.GermanWords;
                        score.TypeId = (int)wordType;
                        score.Created = score.Updated = score.Viewed = DateTime.UtcNow.AddHours(3);
                        score.User.Id = identityUserId;

                        db.Scores.Add(score);
                    }
                }

                db.SaveChanges();
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 
        /// How to embed and access resources by using Visual C# http://support.microsoft.com/kb/319292/en-us
        /// 
        /// 1. Change the "Build Action" property of your XML file from "Content" to "Embedded Resource".
        /// 2. Add "using System.Reflection".
        /// 3. See sample below.
        /// 
        /// </summary>
        public static XDocument XDocument
        {
            get
            {
                if (xDocument == null)
                {
                    Assembly _assembly;
                    StreamReader streamReader;

                    _assembly = Assembly.GetExecutingAssembly();
                    streamReader = new StreamReader(_assembly.GetManifestResourceStream("Ia.Learning.Cl.model.data.germanic.xml"));

                    try
                    {
                        if (streamReader.Peek() != -1)
                        {
                            xDocument = System.Xml.Linq.XDocument.Load(streamReader);
                        }
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                    }
                }

                return xDocument;
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////    
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////   
}
