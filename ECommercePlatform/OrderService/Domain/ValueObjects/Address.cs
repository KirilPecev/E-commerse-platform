using ECommercePlatform.Domain;

using OrderService.Domain.Exceptions;

namespace OrderService.Domain.ValueObjects
{
    public class Address : ValueObject
    {
        public string Street { get; }
        public string City { get; }
        public string ZipCode { get; }
        public string Country { get; }

        public Address(
            string street,
            string city,
            string zipCode,
            string country)
        {
            if (string.IsNullOrWhiteSpace(street))
                throw new OrderDomainException("Street is required.");

            if (string.IsNullOrWhiteSpace(city))
                throw new OrderDomainException("City is required.");

            if (string.IsNullOrWhiteSpace(zipCode))
                throw new OrderDomainException("ZipCode is required.");

            if (string.IsNullOrWhiteSpace(country))
                throw new OrderDomainException("Country is required.");

            Street = street;
            City = city;
            ZipCode = zipCode;
            Country = country;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Street;
            yield return City;
            yield return ZipCode;
            yield return Country;
        }   
    }
}
