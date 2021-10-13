// ***********************************************************************
// Assembly         : ScriptControl
// Author           : 
// Created          : 03-31-2016
//
// Last Modified By : 
// Last Modified On : 03-24-2016
// ***********************************************************************
// <copyright file="S1F12.cs" company="">
//     Copyright ©  2014
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.mirle.ibg3k0.stc.Common;
using com.mirle.ibg3k0.stc.Data.SecsData;

namespace com.mirle.ibg3k0.sc.Data.SECS.OHTC.AT_S
{
    /// <summary>
    /// Class S1F12.
    /// </summary>
    /// <seealso cref="com.mirle.ibg3k0.stc.Data.SecsData.SXFY" />
    public class S1F12 : SXFY
    {
        /// <summary>
        /// The svids
        /// </summary>
        [SecsElement(Index = 1, ListSpreadOut = true)]
        public SVITEM[] SVIDS;

        /// <summary>
        /// Initializes a new instance of the <see cref="S1F12"/> class.
        /// </summary>
        public S1F12() 
        {
            StreamFunction = "S1F12";
            W_Bit = 0;
        }

        /// <summary>
        /// Class SVITEM.
        /// </summary>
        /// <seealso cref="com.mirle.ibg3k0.stc.Data.SecsData.SXFY" />
        public class SVITEM : SXFY
        {
            /// <summary>
            /// The svid
            /// </summary>
            //[SecsElement(Index = 1, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 5)]
            //修改Type為TYPE_2_BYTE_UNSIGNED_INTEGER，並修改長度為1 MarkChou 20190315
            [SecsElement(Index = 1, Type = SecsElement.SecsElementType.TYPE_2_BYTE_UNSIGNED_INTEGER, Length = 1)]
            public string SVID;
            /// <summary>
            /// The svname
            /// </summary>
            //[SecsElement(Index = 2, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 40)]
            [SecsElement(Index = 2, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 64)]//變更長度為64 MarkChou 20190315
            public string SVNAME;
            /// <summary>
            /// The unit
            /// </summary>
            [SecsElement(Index = 3, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 3)]//加入新欄位UNITS MarkChou 20190315
            public string UNITS;
        }
    }
}
