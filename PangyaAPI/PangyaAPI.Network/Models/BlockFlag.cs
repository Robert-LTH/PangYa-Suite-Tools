namespace PangyaAPI.Network.Models
{


    public class BlockFlag
    {
        public BlockFlag()
        {
            if (m_flag == null || (m_flag.ullFlag == 0))
            {
                m_flag = new uFlag(0);
            }

            m_id_state = new IDStateBlockFlag(0);
        }
        public void setIDState(ulong _id_state)
        {
            m_id_state = new IDStateBlockFlag(_id_state);

            // Block Recursos do player
            if ((m_id_state.L_BLOCK_LOUNGE/* & 4*/)) // Block Lounge
                m_flag.lounge = true; // Block Lounge
            if ((m_id_state.L_BLOCK_SHOP_LOUNGE/* & 8*/)) // Block Shop Lounge
                m_flag.personal_shop = true; // Block Shop Lounge
            if ((m_id_state.L_BLOCK_GIFT_SHOP/* & 16*/)) // Block Gift Shop
                m_flag.gift_shop = true; // Block Gift Shop
            if ((m_id_state.L_BLOCK_PAPEL_SHOP/* & 32*/)) // Block Papel Shop
                m_flag.papel_shop = true; // Block Papel Shop
            if ((m_id_state.L_BLOCK_SCRATCHY/* & 64*/)) // Block Scratchy
                m_flag.scratchy = true; // Block Scratchy
            if ((m_id_state.L_BLOCK_TICKER/* & 128*/)) // Block Ticker
                m_flag.ticker = true; // Block Ticker
            if ((m_id_state.L_BLOCK_MEMORIAL_SHOP/* & 256*/)) // Block Memorial Shop
                m_flag.memorial_shop = true; // Block Memorial Shop
        }

        public IDStateBlockFlag m_id_state;
        public uFlag m_flag;
    }

}
