using System.Linq;

namespace Spaces.LBE.PerformanceMonitoring
{
    class BoxFilter
    {
        private float[] m_DataPoints;
        float m_MaxDataValue;
        int m_DataIndex;
        int m_NumDataPoints;
        float m_InvNumDataPoints;


        public BoxFilter(int numDataPoints = 10)
        {
            m_DataPoints = new float[numDataPoints]; // defaults to zero
            m_DataIndex = 0;
            m_MaxDataValue = 0.0f;
            m_NumDataPoints = numDataPoints;
            m_InvNumDataPoints = 1.0f / (float)m_NumDataPoints;
        }


        public void AddDataPoint(float data)
        {
            m_DataPoints[(m_DataIndex++) % m_NumDataPoints] = data;
            if (data > m_MaxDataValue)
            {
                m_MaxDataValue = data;
            }
        }


        public float GetFilteredData()
        {
            return m_DataPoints.Sum() * m_InvNumDataPoints;
        }


        public float GetMaxDataValue()
        {
            return m_MaxDataValue;
        }


        public void ResetMaxDataValue()
        {
            m_MaxDataValue = 0.0f;
        }

    } 

} // namespace Spaces.LBE.PerformanceMonitoring