using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using PangyaAPI.Network.PangyaSession;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.BinaryModels;
using PangyaAPI.Utilities.Log;
namespace PangyaAPI.Network.PangyaPacket
{
    public class packet_func_base
    {
        public static func_arr funcs = new func_arr();      // Cliente
        public static func_arr funcs_sv = new func_arr();   // Server (Retorno)
        public static func_arr funcs_as = new func_arr(); // Auth Server


        public static int MAX_BUFFER_PACKET = 1000;
        public static void MakeBeginPacket(object arg)
        {
            var pd = (ParamDispatch)arg;
            PangyaLog.Write($"Trata pacote {pd._packet.getTipo()}(0x{pd._packet.getTipo():X})", LogDestination.GeneralFile | LogDestination.Console);
        }

        public static void MakeBeginSplitPacket<T>(ushort packetId, Session session, int elementSize, int maxPacket, List<T> elements, bool debug)
        {
            throw new NotSupportedException("Use MakeSplitPacket with an element serializer; generic values cannot be written safely without one.");
        }
        public static void MakeSplitPacket<T>(
       ushort packetId,
       Session session,
       List<T> v_element,
       int elementSize,
       int maxPacket,
       byte tipo, // 0 = short, 1 = uint
       Action<PangyaBinaryWriter, T> addElementToPacket,
       string debug = null)
        {
            if (addElementToPacket == null)
                throw new ArgumentNullException(nameof(addElementToPacket));
            ValidateSplitArguments(session, v_element, elementSize, maxPacket, tipo);
            if (v_element.Count == 0)
                return;

            int elements = v_element.Count;
            int porPacket = ((maxPacket - 100) > elementSize) ? (maxPacket - 100) / elementSize : 1;

            int index = 0;
            int total = elements;

            var it = v_element.GetEnumerator();

            while (index < elements && it.MoveNext())
            {
                PangyaBinaryWriter p = new PangyaBinaryWriter();
                p.init_plain(packetId);

                // MAKE_MED_SPLIT_PACKET equivalent
                int chunkCount = Math.Min(porPacket, elements - index);
                if (tipo == 0)
                {
                    p.WriteInt16((short)total);
                    p.WriteInt16((short)chunkCount);
                }
                else
                {
                    p.WriteUInt32((uint)total);
                    p.WriteUInt32((uint)chunkCount);
                }

                int i = 0;
                do
                {
                    addElementToPacket(p, it.Current);
                    index++;
                    i++;
                } while (i < porPacket && index < elements && it.MoveNext());

                // MAKE_END_SPLIT_PACKET equivalent
                MAKE_SEND_BUFFER(p.GetBytes, session);
                total -= chunkCount;
            }
        }

        public static void MakeSplitPacketFromMap<TKey, TValue>(
            ushort packetId,
            Session session,
            Dictionary<TKey, TValue> v_element,
            int elementSize,
            int maxPacket,
            byte tipo,
            Action<PangyaBinaryWriter, TValue> addElementToPacket,
            string debug = null)
        {
            if (addElementToPacket == null)
                throw new ArgumentNullException(nameof(addElementToPacket));
            if (v_element == null)
                throw new ArgumentNullException(nameof(v_element));
            ValidateSplitArguments(session, new List<TValue>(v_element.Values), elementSize, maxPacket, tipo);
            if (v_element.Count == 0)
                return;

            int elements = v_element.Count;
            int porPacket = ((maxPacket - 100) > elementSize) ? (maxPacket - 100) / elementSize : 1;

            int index = 0;
            int total = elements;

            var it = v_element.Values.GetEnumerator();

            while (index < elements && it.MoveNext())
            {
                var p = new PangyaBinaryWriter();
                p.init_plain(packetId);

                // MAKE_MED_SPLIT_PACKET equivalent
                int chunkCount = Math.Min(porPacket, elements - index);
                if (tipo == 0)
                {
                    p.WriteInt16((short)total);
                    p.WriteInt16((short)chunkCount);
                }
                else
                {
                    p.WriteUInt32((uint)total);
                    p.WriteUInt32((uint)chunkCount);
                }

                int i = 0;
                do
                {
                    addElementToPacket(p, it.Current);
                    index++;
                    i++;
                } while (i < porPacket && index < elements && it.MoveNext());

                // MAKE_END_SPLIT_PACKET_REF equivalent
                MAKE_SEND_BUFFER(p.GetBytes, session);
                total -= chunkCount;
            }
        }

        private static void ValidateSplitArguments<T>(Session session, ICollection<T> elements,
            int elementSize, int maxPacket, byte countType)
        {
            if (session == null)
                throw new ArgumentNullException(nameof(session));
            if (elements == null)
                throw new ArgumentNullException(nameof(elements));
            if (elementSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(elementSize));
            if (maxPacket <= 100)
                throw new ArgumentOutOfRangeException(nameof(maxPacket));
            if (countType > 1)
                throw new ArgumentOutOfRangeException(nameof(countType));
            if (countType == 0 && elements.Count > short.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(elements), "A 16-bit packet count cannot represent this collection.");
        }
        public static void MAKE_SEND_BUFFER(byte[] rawPacket, Session _session)
        {
            if (rawPacket == null || rawPacket.Length == 0)
                throw new ArgumentException("Packet buffer cannot be null or empty.", nameof(rawPacket));
            if (_session == null)
                throw new ArgumentNullException(nameof(_session));
            if (_session.m_sock == null || !_session.m_sock.Connected)
                throw new InvalidOperationException("Cannot send a packet through a disconnected session.");

            _session.requestSendBuffer(rawPacket);
            if (_session.devolve())
                _session.Disconnect();
        }
    }
}
