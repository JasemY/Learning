using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Ia.Learning.Cl.Model
{
    ////////////////////////////////////////////////////////////////////////////

    /// <summary publish="true">
    /// Score entity functions
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
    public partial class Score
    {
        /// <summary/>
        public Score()
        {
        }

        /// <summary/>
        public int Id { get; set; }

        /// <summary/>
        public int TestId { get; set; }

        /// <summary/>
        public int TypeId { get; set; }

        /// <summary/>
        public string Question { get; set; }

        /// <summary/>
        public string Answer { get; set; }

        /// <summary/>
        public int NumberOfTimesAsked { get; set; }

        /// <summary/>
        public int CumulativeAnswerCorrectnessIndicator { get; set; }

        /// <summary/>
        public int NumberOfConsecutiveCorrects { get; set; }

        /// <summary/>
        public DateTime Created { get; set; }

        /// <summary/>
        public DateTime Updated { get; set; }

        /// <summary/>
        public DateTime Viewed { get; set; }

        /// <summary/>
        public virtual IdentityUser User { get; set; }

        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Read all tests
        /// </summary>
        public static List<Score> ReadList()
        {
            List<Score> list;

            using (var db = new Ia.Learning.Cl.Model.Learning())
            {
                list = (from q in db.Scores select q).ToList();
            }

            return list;
        }

        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        ///
        /// </summary>
        public static List<Score> GermanWordScoreList(string identityUserId)
        {
            return ScoreList(Ia.Learning.Cl.Model.Business.Test.TestTopic.GermanWords, identityUserId);
        }

        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        ///
        /// </summary>
        public static List<Score> MultiplicationScoreList(string identityUserId)
        {
            return ScoreList(Ia.Learning.Cl.Model.Business.Test.TestTopic.Multiplication, identityUserId);
        }

        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        ///
        /// </summary>
        public static List<Score> KanjiScoreList(string identityUserId)
        {
            return ScoreList(Ia.Learning.Cl.Model.Business.Test.TestTopic.Kanji, identityUserId);
        }

        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        ///
        /// </summary>
        [Obsolete("Not used. ")]
        private static List<Score> ScoreList(Ia.Learning.Cl.Model.Business.Test.TestTopic testList)
        {
            List<Score> list;

            using (var db = new Ia.Learning.Cl.Model.Learning())
            {
                list = (from q in db.Scores where q.TestId == (int)testList select q).ToList();
            }

            return list;
        }

        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        ///
        /// </summary>
        private static List<Score> ScoreList(Ia.Learning.Cl.Model.Business.Test.TestTopic testList, string identityUserId)
        {
            List<Score> list;

            using (var db = new Ia.Learning.Cl.Model.Learning())
            {
                list = (from q in db.Scores where q.TestId == (int)testList && q.User.Id == identityUserId select q).ToList();
            }

            return list;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////
}