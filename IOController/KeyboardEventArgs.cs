using System;
namespace ControlEngine
{
    public sealed class CKeyboardEventArgs : EventArgs
    {
        private CKeys m_keys;

        public CKeyboardEventArgs(CKeys keys)
        {
            this.m_keys = keys;
        }

        public CKeys getKey()
        {
            return m_keys;
        }
    }
}