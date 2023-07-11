using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NssIT.Kiosk.Client.ViewPage.Trip
{
    public class TripDetailViewRow : ViewModelBase
    {
        public string _vehicleService;
        /// <summary>
        /// Like : ETS / INTERCITY
        /// </summary>
        public string VehicleService
        {
            get
            {
                return _vehicleService;
            }
            set
            {
                if (_vehicleService != value)
                {
                    _vehicleService = value;
                    this.OnPropertyChanged("VehicleService");
                }
            }
        }

        public string _tripId;
        public string TripId
        {
            get
            {
                return _tripId;
            }
            set
            {
                if (_tripId != value)
                {
                    _tripId = value;
                    this.OnPropertyChanged("TripId");
                }
            }
        }

        public string _vehicleNo;
        public string VehicleNo
        {
            get
            {
                return _vehicleNo;
            }
            set
            {
                if (_vehicleNo != value)
                {
                    _vehicleNo = value;
                    this.OnPropertyChanged("VehicleNo");
                }
            }
        }

        public string _serviceCategory;
        /// <summary>
        /// Like "Gold", or "Platinum"
        /// </summary>
        public string ServiceCategory
        {
            get
            {
                return _serviceCategory;
            }
            set
            {
                if (_serviceCategory != value)
                {
                    _serviceCategory = value;
                    this.OnPropertyChanged("ServiceCategory");
                }
            }
        }

        public string _departDateStr;
        public string DepartDateStr
        {
            get
            {
                return _departDateStr;
            }
            set
            {
                if (_departDateStr != value)
                {
                    _departDateStr = value;
                    this.OnPropertyChanged("DepartDateStr");
                }
            }
        }

        public string _departTimeStr;
        public string DepartTimeStr
        {
            get
            {
                return _departTimeStr;
            }
            set
            {
                if (_departTimeStr != value)
                {
                    _departTimeStr = value;
                    this.OnPropertyChanged("DepartTimeStr");
                }
            }
        }

        public int _arrivalDayOffset;
        public int ArrivalDayOffset
        {
            get
            {
                return _arrivalDayOffset;
            }
            set
            {
                if (_arrivalDayOffset != value)
                {
                    _arrivalDayOffset = value;
                    this.OnPropertyChanged("ArrivalDayOffset");
                }
            }
        }

        public string _arrivalDayOffsetStr;
        public string ArrivalDayOffsetStr
        {
            get
            {
                return _arrivalDayOffsetStr;
            }
            set
            {
                if (_arrivalDayOffsetStr != value)
                {
                    _arrivalDayOffsetStr = value;
                    this.OnPropertyChanged("ArrivalDayOffsetStr");
                }
            }
        }

        public string _arrivalTimeStr;
        public string ArrivalTimeStr
        {
            get
            {
                return _arrivalTimeStr;
            }
            set
            {
                if (_arrivalTimeStr != value)
                {
                    _arrivalTimeStr = value;
                    this.OnPropertyChanged("ArrivalTimeStr");
                }
            }
        }

        public string _currency;
        public string Currency
        {
            get
            {
                return _currency;
            }
            set
            {
                if (_currency != value)
                {
                    _currency = value;
                    this.OnPropertyChanged("Currency");
                }
            }
        }

        public string _priceStr;
        public string PriceStr
        {
            get
            {
                return _priceStr;
            }
            set
            {
                if (_priceStr != value)
                {
                    _priceStr = value;
                    this.OnPropertyChanged("PriceStr");
                }
            }
        }

        public int _availableSeat;
        public int AvailableSeat
        {
            get
            {
                return _availableSeat;
            }
            set
            {
                if (_availableSeat != value)
                {
                    _availableSeat = value;
                    this.OnPropertyChanged("AvailableSeat");
                }
            }
        }

        public DateTime _departDateTime;
        public DateTime DepartDateTime
        {
            get
            {
                return _departDateTime;
            }
            set
            {
                if (_departDateTime != value)
                {
                    _departDateTime = value;
                    this.OnPropertyChanged("DepartDateTime");
                }
            }
        }

        public DateTime _arrivalDateTime;
        public DateTime ArrivalDateTime
        {
            get
            {
                return _arrivalDateTime;
            }
            set
            {
                if (_arrivalDateTime != value)
                {
                    _arrivalDateTime = value;
                    this.OnPropertyChanged("ArrivalDateTime");
                }
            }
        }

        private decimal _price;
        public decimal Price
        {
            get
            {
                return _price;
            }
            set
            {
                if (_price != value)
                {
                    _price = value;
                    this.OnPropertyChanged("Price");
                }
            }
        }

        public Visibility _soldOutVisible;
        public Visibility SoldOutVisible
        {
            get
            {
                return _soldOutVisible;
            }
            set
            {
                if (_soldOutVisible != value)
                {
                    _soldOutVisible = value;
                    this.OnPropertyChanged("SoldOutVisible");
                }
            }
        }

        public Visibility _isPriceVisible;
        public Visibility IsPriceVisible
        {
            get
            {
                return _isPriceVisible;
            }
            set
            {
                if (_isPriceVisible != value)
                {
                    _isPriceVisible = value;
                    this.OnPropertyChanged("IsPriceVisible");
                }
            }
        }

        public Visibility _isAvailableSeatVisible;
        public Visibility IsAvailableSeatVisible
        {
            get
            {
                return _isAvailableSeatVisible;
            }
            set
            {
                if (_isAvailableSeatVisible != value)
                {
                    _isAvailableSeatVisible = value;
                    this.OnPropertyChanged("IsAvailableSeatVisible");
                }
            }
        }

        public Visibility _isPickSeatVisible;
        public Visibility IsPickSeatVisible
        {
            get
            {
                return _isPickSeatVisible;
            }
            set
            {
                if (_isPickSeatVisible != value)
                {
                    _isPickSeatVisible = value;
                    this.OnPropertyChanged("IsPickSeatVisible");
                }
            }
        }

        public Visibility _isNotEnoughPaxSeatVisible;
        public Visibility IsNotEnoughPaxSeatVisible
        {
            get
            {
                return _isNotEnoughPaxSeatVisible;
            }
            set
            {
                if (_isNotEnoughPaxSeatVisible != value)
                {
                    _isNotEnoughPaxSeatVisible = value;
                    this.OnPropertyChanged("IsNotEnoughPaxSeatVisible");
                }
            }
        }


        public Visibility _quickFinishSeatAvailableVisible;
        public Visibility QuickFinishSeatAvailableVisible
        {
            get
            {
                return _quickFinishSeatAvailableVisible;
            }
            set
            {
                if (_quickFinishSeatAvailableVisible != value)
                {
                    _quickFinishSeatAvailableVisible = value;
                    this.OnPropertyChanged("QuickFinishSeatAvailableVisible");
                }
            }
        }
    }
}