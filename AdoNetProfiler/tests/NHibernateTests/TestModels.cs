using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Mapping.ByCode.Conformist;

namespace NHibernateTests
{
    //MODEL
    public class Customer
    {
        //CUSTOMER
        public int Id { get; set; } //CUSTOMER_ID
        public string CustomerCode { get; set; } //CUSTOMER_CODE
        public string CompanyName { get; set; } //COMPANY_NAME
        public string ContactName { get; set; } //CONTACT_NAME
        public string ContactTitle { get; set; } //CONTACT_TITLE
        public string Address { get; set; } //ADDRESS
        public string City { get; set; } //CITY
        public string Region { get; set; } //REGION
        public string PostalCode { get; set; } //POSTAL_CODE
        public string Country { get; set; } //COUNTRY
        public string Phone { get; set; } //PHONE
        public string Fax { get; set; } //FAX
    }


    //NHIBERNATE MAPPING
    internal class CustomerMapping : ClassMapping<Customer>
    {
        public CustomerMapping()
        {
            //Schema("TODO");
            Table("CUSTOMER");
            Lazy(false);
            Id(prop => prop.Id, map =>
            {
                map.Column("CUSTOMER_ID");
                //map.Generator(Generators.Sequence, gmap => gmap.Params(new {sequence = "DATA_FILE_ID_SEQ"}));
            });
            Property(prop => prop.CustomerCode, map => map.Column("CUSTOMER_CODE"));
            Property(prop => prop.CompanyName, map => map.Column("COMPANY_NAME"));
            Property(prop => prop.ContactName, map => map.Column("CONTACT_NAME"));
            Property(prop => prop.ContactTitle, map => map.Column("CONTACT_TITLE"));
            Property(prop => prop.Address, map => map.Column("ADDRESS"));
            Property(prop => prop.City, map => map.Column("CITY"));
            Property(prop => prop.Region, map => map.Column("REGION"));
            Property(prop => prop.PostalCode, map => map.Column("POSTAL_CODE"));
            Property(prop => prop.Country, map => map.Column("COUNTRY"));
            Property(prop => prop.Phone, map => map.Column("PHONE"));
            Property(prop => prop.Fax, map => map.Column("FAX"));
        }
    }
}
