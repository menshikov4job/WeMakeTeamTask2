namespace WeMakeTeamTask2
{
    public class Entity
    {
        bool _idIsSet;
        bool _operDateIsSet;
        bool _amountIsSet;

        Guid _id;
        DateTimeOffset _operationDate;
        decimal _amount;

        
        public Guid Id 
        {
            get => _id;
            set
            {
                _id = value;
                _idIsSet = true;
            }
        }

        public DateTimeOffset OperationDate 
        {
            get => _operationDate;            
            set
            { 
                _operationDate = value;
                _operDateIsSet = true;
            } 
        }
        public decimal Amount 
        {
            get => _amount; 
            set
            {
                _amount = value;
                _amountIsSet = true;
            }
        }

        public Entity()
        {
            this.Id = Guid.NewGuid();
            this.OperationDate = DateTime.Now;
            _idIsSet = false;
            _operDateIsSet = false;
        }

        public string ValidateSetFields()
        {
            // Имена полей можно было через reflection получить
            string errMsg = _idIsSet ? "" : "Id не установлено";
            if (!_operDateIsSet)
                errMsg += ";OperationDate не установлено";
            if(!_amountIsSet)
             errMsg += ";Amount не установлено";
            
            return errMsg.TrimStart(';');
        }
    }
}
