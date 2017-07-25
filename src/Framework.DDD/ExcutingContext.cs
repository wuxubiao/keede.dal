using System;
using System.Threading;
using Keede.DAL.DomainBase.Unitwork;


namespace Framework.DDD
{
    public class ExcutingContext
    {
        private static readonly ThreadLocal<IUnitOfWork> _unitWork = new ThreadLocal<IUnitOfWork>();

        public static UnitOfWork UseSqlServerUnitOfWork()
        {
            if (_unitWork.Value != null)
                throw new ApplicationException("当前线程已经启动了一个工作单元");

            var unitWork = new SqlServerUnitOfWork();
            _unitWork.Value = unitWork;

            unitWork.CommittedEvent += CommittedEventHandle;
            unitWork.RollBackEvent += RollBackEventHandle;
            
            return unitWork;
        }

        public static SqlServerUnitOfWork GetCurrentUnitOfWork()
        {
            return _unitWork.Value as SqlServerUnitOfWork;
        }

        private static void CommittedEventHandle(bool isSuccess)
        {
            _unitWork.Value = null;
        }

        private static void RollBackEventHandle()
        {
            _unitWork.Value = null;
        }
    }
}
