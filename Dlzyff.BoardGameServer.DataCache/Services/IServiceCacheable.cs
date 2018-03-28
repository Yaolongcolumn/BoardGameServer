using System.Collections.Generic;

namespace Dlzyff.BoardGameServer.DataCache.Services
{
    public interface IServiceCacheable
    {
        void InitCardsData();

        void ResetCards();

        string GetRandomCard();
    }
}
