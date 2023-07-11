using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyKadTest
{
    public class PassengerIdentity
    {
        public bool IsIDReadSuccess { get; private set; } = false;
        public string PassportNumber { get; private set; } = null;
        public string IdNumber { get; private set; } = null;
        public string Name { get; private set; } = null;
        public string Message { get; private set; } = null;
        public Gender Sex { get; private set; } = Gender.Male;
        public DateTime DateOfBirth { get; private set; } = DateTime.MinValue;
        public int Age { get; private set; } = 1;
        public string PNRNo { get; private set; } = null;
        public TicketTypeModel[] PNRTicketTypeList { get; private set; } = null;

        public PassengerIdentity(bool isIDReadSuccess, string passportNumber, string idNumber,
            string name, DateTime? birthdate, Gender gender, string message)
        {
            DateTime today = DateTime.Now;
            IsIDReadSuccess = isIDReadSuccess;
            PassportNumber = string.IsNullOrWhiteSpace(passportNumber) ? null : passportNumber.Trim();
            IdNumber = string.IsNullOrWhiteSpace(idNumber) ? null : idNumber.Trim();
            Name = TrimName(name);
            Message = string.IsNullOrWhiteSpace(message) ? null : message.Trim();
            DateOfBirth = birthdate.HasValue ? birthdate.Value : DateOfBirth;
            Sex = gender;

            if (birthdate.HasValue)
            {
                Age = DateTime.Now.Year - DateOfBirth.Year;

                if (DateOfBirth.Date > today.AddYears(-Age)) Age--;

                if (Age <= 0)
                    Age = 1;
            }

            if (IsIDReadSuccess)
            {
                if (Name is null)
                {
                    IsIDReadSuccess = false;
                    Message = (string.IsNullOrWhiteSpace(Message) ? "" : $@"{Message};") + $@"Name not found";
                }
                if ((PassportNumber is null) && (IdNumber is null))
                {
                    IsIDReadSuccess = false;
                    Message = (string.IsNullOrWhiteSpace(Message) ? "" : $@"{Message};") + $@"Identity Number not found";
                }
                if (!birthdate.HasValue)
                {
                    IsIDReadSuccess = false;
                    Message = (string.IsNullOrWhiteSpace(Message) ? "" : $@"{Message};") + $@"Invalid Date of Birth";
                }
            }
            else
            {
                if (Message is null)
                {
                    Message = "No data found-*.";
                }
            }
        }

        public void SetPNR(string pnrNo, TicketTypeModel[] pnrTicketTypeList)
        {
            if (pnrTicketTypeList?.Length > 0)
            {
                PNRNo = pnrNo;
                PNRTicketTypeList = pnrTicketTypeList;
            }
        }

        private string TrimName(string nameX)
        {
            if (nameX is null)
                return null;

            nameX = nameX.Trim();
            for (int ct = 0; ct < 5; ct++)
                nameX = nameX.Replace("  ", " ");

            if (nameX.Length >= 100)
                nameX = nameX.Substring(0, 99);

            return nameX;
        }

    }
}