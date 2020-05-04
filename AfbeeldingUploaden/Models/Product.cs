using System.ComponentModel.DataAnnotations;

namespace AfbeeldingUploaden.Models
{
    public class Product
    {
        /// <summary>
        /// De primaire sleutel
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// De naam van het Product.
        /// <para>De naam is verplicht, en mag maximaal 80 tekens groot zijn</para>
        /// </summary>
        [Required, StringLength(80)]
        public string Naam { get; set; }

        /// <summary>
        /// De omschrijving van het product.
        /// <papar>De omschrijving mag maximaal 255 tekens groot zijn.</papar>
        /// </summary>
        public string Omschrijving { get; set; }

        /// <summary>
        /// De naam van de afbeelding.
        /// <para>We slaan de afbeelding op in de map 'img'. Met deze naam kunnen we de afbeeling terugvinden.</para>
        /// <para>De afbeeldingsnaam mag maximaal 255 tekens groot zijn</para>
        /// </summary>
        [StringLength(255)]
        public string Afbeelding { get; set; }
    }
}
