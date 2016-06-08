﻿//******************************************************************************************************
//  Mapper.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  06/01/2016 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Collections.Generic;
using System.Data;
using AlgorithmTemplate.Framework;
using GSF.TimeSeries;

namespace AlgorithmTemplate.Model
{
    /// <summary>
    /// Stub mapping class to be replaced by compiler-generated code.
    /// </summary>
    public class Mapper
    {
        #region [ Members ]

        // Fields
        private SignalLookup m_lookup;

        #endregion

        #region [ Constructors ]

        public Mapper(SignalLookup lookup)
        {
            m_lookup = lookup;
        }

        #endregion

        #region [ Properties ]

        public SignalLookup Lookup
        {
            get
            {
                return m_lookup;
            }
        }

        #endregion

        #region [ Methods ]

        public void CrunchMetadata(DataSet metadata)
        {
            m_lookup.CrunchMetadata(metadata);
        }

        public void Map(IDictionary<MeasurementKey, IMeasurement> measurements)
        {
            m_lookup.UpdateMeasurementLookup(measurements);
        }

        #endregion
    }
}
