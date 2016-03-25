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
    /// Math data support functions
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
    public partial class Math
    {
        public enum WordType { Multiplication = 1 };

        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        ///
        /// </summary>
        public Math() { }

        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        ///
        /// </summary>
        public static void UpdateTestDatabaseWithMultiplication(string identityUserId)
        {
            string question, answer;
            Ia.Learning.Cl.Model.Score score;
            List<Ia.Learning.Cl.Model.Score> scoreList;

            // below: synch database words with XML list
            using (var db = new Ia.Learning.Cl.Model.Learning())
            {
                scoreList = Ia.Learning.Cl.Model.Score.MultiplicationScoreList(identityUserId);

                // below:
                foreach (string s in MultiplicationHashtable.Keys)
                {
                    question = s;
                    answer = MultiplicationHashtable[s].ToString();

                    score = (from q in scoreList where q.TestId == (int)Ia.Learning.Cl.Model.Business.Test.TestTopic.Multiplication && q.Question == question && q.Answer == answer && q.TypeId == (int)WordType.Multiplication select q).FirstOrDefault();

                    if (score == null)
                    {
                        score = new Ia.Learning.Cl.Model.Score();

                        score.Question = question;
                        score.Answer = answer;
                        score.TestId = (int)Ia.Learning.Cl.Model.Business.Test.TestTopic.Multiplication;
                        score.TypeId = (int)Ia.Learning.Cl.Model.Data.Math.WordType.Multiplication;
                        score.Created = score.Updated = score.Viewed = DateTime.UtcNow.AddHours(3);
                        score.User.Id = identityUserId;

                        db.Scores.Add(score);
                    }
                }

                db.SaveChanges();
            }
        }

        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 
        /// </summary>
        public static Hashtable MultiplicationHashtable
        {
            get
            {
                int i, j;
                string s;
                Hashtable multiplicationHashtable;

                multiplicationHashtable = new Hashtable(169); // 13*13=169

                for (i = 0; i <= 12; i++)
                {
                    for (j = 0; j <= 12; j++)
                    {
                        s = i + "*" + j;
                        multiplicationHashtable[s] = i * j;
                    }
                }

                return multiplicationHashtable;
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////    
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////   
}
