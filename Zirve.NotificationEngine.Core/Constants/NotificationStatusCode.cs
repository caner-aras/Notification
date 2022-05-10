namespace Zirve.NotificationEngine.Core.Constants
{
    public class NotificationStatusCode
    {
        /// <summary>
        /// Gönderiliyor
        /// </summary>
        public const int Pending = 0;

        /// <summary>
        /// Sorgulanıyor
        /// </summary>
        public const int Processing = 1;

        /// <summary>
        /// Gönderilmeyi Bekliyor
        /// </summary>
        public const int Waiting = 2;

        /// <summary>
        /// Gönderilemedi
        /// </summary>
        public const int Error = 3;

        /// <summary>
        /// Gönderildi
        /// </summary>
        public const int Completed = 4;

        /// <summary>
        /// Gönderildi sorgulanmayı bekliyor
        /// </summary>
        public const int SendedWaitingInquiry = 5;
        /// <summary>
        /// Okundu
        /// </summary>
        public const int Readed = 6;
    }
}
