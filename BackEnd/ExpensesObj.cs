﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd
{
    public class ExpensesObj
    {
        private Guid _idGuid;
        private string _category;
        private string _subCategory;
        private double _expectedAmount;
        private double _actualAmount;
        private string _description;

        public delegate void VoidDelegate(Guid idGuid);
        public event VoidDelegate ExpensesObjChanged;

        public Guid IdGuid
        {
            get { return _idGuid; }
            set { _idGuid = value; }
        }
        public string Category
        {
            get { return _category; }
            set
            {
                if (value != null)
                {
                    _category = value;
                    OnExpensesObjChanged();
                }
            }
        }
        public string SubCategory
        {
            get { return _subCategory; }
            set
            {
                if (value != null)
                {
                    _subCategory = value;
                    OnExpensesObjChanged();
                }
            }
        }
        public double ExpectedAmount
        {
            get { return _expectedAmount; }
            set
            {
                _expectedAmount = value;
                OnExpensesObjChanged();
            }
        }
        public double ActualAmount
        {
            get { return _actualAmount; }
            set
            {
                _actualAmount = value;
                OnExpensesObjChanged();
            }
        }
        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                OnExpensesObjChanged();
            }
        }

        public ExpensesObj(string category, string subCategory, double expectedAmount, double actualAmount, System.Guid idGuid,
            string description = null)
        {
            IdGuid = idGuid == Guid.Empty ? Guid.NewGuid() : idGuid;
            Category = category;
            SubCategory = subCategory;
            ExpectedAmount = expectedAmount;
            ActualAmount = actualAmount;
            Description = description;
        }
        protected virtual void OnExpensesObjChanged()
        {
            if (ExpensesObjChanged != null)
            {
                ExpensesObjChanged(this.IdGuid);
            }
        }
    }

}
