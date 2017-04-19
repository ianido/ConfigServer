using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using yupisoft.ConfigServer.Core.Utils;

namespace yupisoft.ConfigServer.Core.Services
{
    public class GeoServices
    {
        private IGeoIPServiceProvider _geoServiceProvider;
        private IMemoryCache _memoryCache;

        private Hashtable countriesByContinents;

        public GeoServices(IGeoIPServiceProvider geoServiceProvider, IMemoryCache memoryCache)
        {
            _geoServiceProvider = geoServiceProvider;
            _memoryCache = memoryCache;
            FillCountriesAndContinents();
        }

        private void FillCountriesAndContinents()
        {
            countriesByContinents = new Hashtable(504);
            countriesByContinents.Add("AFG", "AS");
            countriesByContinents.Add("ALB", "EU");
            countriesByContinents.Add("DZA", "AF");
            countriesByContinents.Add("ASM", "OC");
            countriesByContinents.Add("AND", "EU");
            countriesByContinents.Add("AGO", "AF");
            countriesByContinents.Add("AIA", "NA");
            countriesByContinents.Add("ATA", "AN");
            countriesByContinents.Add("ATG", "NA");
            countriesByContinents.Add("ARG", "SA");
            countriesByContinents.Add("ARM", "AS");
            countriesByContinents.Add("ABW", "NA");
            countriesByContinents.Add("AUS", "OC");
            countriesByContinents.Add("AUT", "EU");
            countriesByContinents.Add("AZE", "AS");
            countriesByContinents.Add("BHS", "NA");
            countriesByContinents.Add("BHR", "AS");
            countriesByContinents.Add("BGD", "AS");
            countriesByContinents.Add("BRB", "NA");
            countriesByContinents.Add("BLR", "EU");
            countriesByContinents.Add("BEL", "EU");
            countriesByContinents.Add("BLZ", "NA");
            countriesByContinents.Add("BEN", "AF");
            countriesByContinents.Add("BMU", "NA");
            countriesByContinents.Add("BTN", "AS");
            countriesByContinents.Add("BOL", "SA");
            countriesByContinents.Add("BES", "NA");
            countriesByContinents.Add("BIH", "EU");
            countriesByContinents.Add("BWA", "AF");
            countriesByContinents.Add("BVT", "AN");
            countriesByContinents.Add("BRA", "SA");
            countriesByContinents.Add("IOT", "AS");
            countriesByContinents.Add("VGB", "NA");
            countriesByContinents.Add("BRN", "AS");
            countriesByContinents.Add("BGR", "EU");
            countriesByContinents.Add("BFA", "AF");
            countriesByContinents.Add("BDI", "AF");
            countriesByContinents.Add("KHM", "AS");
            countriesByContinents.Add("CMR", "AF");
            countriesByContinents.Add("CAN", "NA");
            countriesByContinents.Add("CPV", "AF");
            countriesByContinents.Add("CYM", "NA");
            countriesByContinents.Add("CAF", "AF");
            countriesByContinents.Add("TCD", "AF");
            countriesByContinents.Add("CHL", "SA");
            countriesByContinents.Add("CHN", "AS");
            countriesByContinents.Add("CXR", "OC");
            countriesByContinents.Add("CCK", "AS");
            countriesByContinents.Add("COL", "SA");
            countriesByContinents.Add("COM", "AF");
            countriesByContinents.Add("COK", "OC");
            countriesByContinents.Add("CRI", "NA");
            countriesByContinents.Add("HRV", "EU");
            countriesByContinents.Add("CUB", "NA");
            countriesByContinents.Add("CUW", "NA");
            countriesByContinents.Add("CYP", "EU");
            countriesByContinents.Add("CZE", "EU");
            countriesByContinents.Add("COD", "AF");
            countriesByContinents.Add("DNK", "EU");
            countriesByContinents.Add("DJI", "AF");
            countriesByContinents.Add("DMA", "NA");
            countriesByContinents.Add("DOM", "NA");
            countriesByContinents.Add("TLS", "OC");
            countriesByContinents.Add("ECU", "SA");
            countriesByContinents.Add("EGY", "AF");
            countriesByContinents.Add("SLV", "NA");
            countriesByContinents.Add("GNQ", "AF");
            countriesByContinents.Add("ERI", "AF");
            countriesByContinents.Add("EST", "EU");
            countriesByContinents.Add("ETH", "AF");
            countriesByContinents.Add("FLK", "SA");
            countriesByContinents.Add("FRO", "EU");
            countriesByContinents.Add("FJI", "OC");
            countriesByContinents.Add("FIN", "EU");
            countriesByContinents.Add("FRA", "EU");
            countriesByContinents.Add("GUF", "SA");
            countriesByContinents.Add("PYF", "OC");
            countriesByContinents.Add("ATF", "AN");
            countriesByContinents.Add("GAB", "AF");
            countriesByContinents.Add("GMB", "AF");
            countriesByContinents.Add("GEO", "AS");
            countriesByContinents.Add("DEU", "EU");
            countriesByContinents.Add("GHA", "AF");
            countriesByContinents.Add("GIB", "EU");
            countriesByContinents.Add("GRC", "EU");
            countriesByContinents.Add("GRL", "NA");
            countriesByContinents.Add("GRD", "NA");
            countriesByContinents.Add("GLP", "NA");
            countriesByContinents.Add("GUM", "OC");
            countriesByContinents.Add("GTM", "NA");
            countriesByContinents.Add("GGY", "EU");
            countriesByContinents.Add("GIN", "AF");
            countriesByContinents.Add("GNB", "AF");
            countriesByContinents.Add("GUY", "SA");
            countriesByContinents.Add("HTI", "NA");
            countriesByContinents.Add("HMD", "AN");
            countriesByContinents.Add("HND", "NA");
            countriesByContinents.Add("HKG", "AS");
            countriesByContinents.Add("HUN", "EU");
            countriesByContinents.Add("ISL", "EU");
            countriesByContinents.Add("IND", "AS");
            countriesByContinents.Add("IDN", "AS");
            countriesByContinents.Add("IRN", "AS");
            countriesByContinents.Add("IRQ", "AS");
            countriesByContinents.Add("IRL", "EU");
            countriesByContinents.Add("IMN", "EU");
            countriesByContinents.Add("ISR", "AS");
            countriesByContinents.Add("ITA", "EU");
            countriesByContinents.Add("CIV", "AF");
            countriesByContinents.Add("JAM", "NA");
            countriesByContinents.Add("JPN", "AS");
            countriesByContinents.Add("JEY", "EU");
            countriesByContinents.Add("JOR", "AS");
            countriesByContinents.Add("KAZ", "AS");
            countriesByContinents.Add("KEN", "AF");
            countriesByContinents.Add("KIR", "OC");
            countriesByContinents.Add("XKX", "EU");
            countriesByContinents.Add("KWT", "AS");
            countriesByContinents.Add("KGZ", "AS");
            countriesByContinents.Add("LAO", "AS");
            countriesByContinents.Add("LVA", "EU");
            countriesByContinents.Add("LBN", "AS");
            countriesByContinents.Add("LSO", "AF");
            countriesByContinents.Add("LBR", "AF");
            countriesByContinents.Add("LBY", "AF");
            countriesByContinents.Add("LIE", "EU");
            countriesByContinents.Add("LTU", "EU");
            countriesByContinents.Add("LUX", "EU");
            countriesByContinents.Add("MAC", "AS");
            countriesByContinents.Add("MKD", "EU");
            countriesByContinents.Add("MDG", "AF");
            countriesByContinents.Add("MWI", "AF");
            countriesByContinents.Add("MYS", "AS");
            countriesByContinents.Add("MDV", "AS");
            countriesByContinents.Add("MLI", "AF");
            countriesByContinents.Add("MLT", "EU");
            countriesByContinents.Add("MHL", "OC");
            countriesByContinents.Add("MTQ", "NA");
            countriesByContinents.Add("MRT", "AF");
            countriesByContinents.Add("MUS", "AF");
            countriesByContinents.Add("MYT", "AF");
            countriesByContinents.Add("MEX", "NA");
            countriesByContinents.Add("FSM", "OC");
            countriesByContinents.Add("MDA", "EU");
            countriesByContinents.Add("MCO", "EU");
            countriesByContinents.Add("MNG", "AS");
            countriesByContinents.Add("MNE", "EU");
            countriesByContinents.Add("MSR", "NA");
            countriesByContinents.Add("MAR", "AF");
            countriesByContinents.Add("MOZ", "AF");
            countriesByContinents.Add("MMR", "AS");
            countriesByContinents.Add("NAM", "AF");
            countriesByContinents.Add("NRU", "OC");
            countriesByContinents.Add("NPL", "AS");
            countriesByContinents.Add("NLD", "EU");
            countriesByContinents.Add("ANT", "NA");
            countriesByContinents.Add("NCL", "OC");
            countriesByContinents.Add("NZL", "OC");
            countriesByContinents.Add("NIC", "NA");
            countriesByContinents.Add("NER", "AF");
            countriesByContinents.Add("NGA", "AF");
            countriesByContinents.Add("NIU", "OC");
            countriesByContinents.Add("NFK", "OC");
            countriesByContinents.Add("PRK", "AS");
            countriesByContinents.Add("MNP", "OC");
            countriesByContinents.Add("NOR", "EU");
            countriesByContinents.Add("OMN", "AS");
            countriesByContinents.Add("PAK", "AS");
            countriesByContinents.Add("PLW", "OC");
            countriesByContinents.Add("PSE", "AS");
            countriesByContinents.Add("PAN", "NA");
            countriesByContinents.Add("PNG", "OC");
            countriesByContinents.Add("PRY", "SA");
            countriesByContinents.Add("PER", "SA");
            countriesByContinents.Add("PHL", "AS");
            countriesByContinents.Add("PCN", "OC");
            countriesByContinents.Add("POL", "EU");
            countriesByContinents.Add("PRT", "EU");
            countriesByContinents.Add("PRI", "NA");
            countriesByContinents.Add("QAT", "AS");
            countriesByContinents.Add("COG", "AF");
            countriesByContinents.Add("ROU", "EU");
            countriesByContinents.Add("RUS", "EU");
            countriesByContinents.Add("RWA", "AF");
            countriesByContinents.Add("REU", "AF");
            countriesByContinents.Add("BLM", "NA");
            countriesByContinents.Add("SHN", "AF");
            countriesByContinents.Add("KNA", "NA");
            countriesByContinents.Add("LCA", "NA");
            countriesByContinents.Add("MAF", "NA");
            countriesByContinents.Add("SPM", "NA");
            countriesByContinents.Add("VCT", "NA");
            countriesByContinents.Add("WSM", "OC");
            countriesByContinents.Add("SMR", "EU");
            countriesByContinents.Add("SAU", "AS");
            countriesByContinents.Add("SEN", "AF");
            countriesByContinents.Add("SRB", "EU");
            countriesByContinents.Add("SCG", "EU");
            countriesByContinents.Add("SYC", "AF");
            countriesByContinents.Add("SLE", "AF");
            countriesByContinents.Add("SGP", "AS");
            countriesByContinents.Add("SXM", "NA");
            countriesByContinents.Add("SVK", "EU");
            countriesByContinents.Add("SVN", "EU");
            countriesByContinents.Add("SLB", "OC");
            countriesByContinents.Add("SOM", "AF");
            countriesByContinents.Add("ZAF", "AF");
            countriesByContinents.Add("SGS", "AN");
            countriesByContinents.Add("KOR", "AS");
            countriesByContinents.Add("SSD", "AF");
            countriesByContinents.Add("ESP", "EU");
            countriesByContinents.Add("LKA", "AS");
            countriesByContinents.Add("SDN", "AF");
            countriesByContinents.Add("SUR", "SA");
            countriesByContinents.Add("SJM", "EU");
            countriesByContinents.Add("SWZ", "AF");
            countriesByContinents.Add("SWE", "EU");
            countriesByContinents.Add("CHE", "EU");
            countriesByContinents.Add("SYR", "AS");
            countriesByContinents.Add("STP", "AF");
            countriesByContinents.Add("TWN", "AS");
            countriesByContinents.Add("TJK", "AS");
            countriesByContinents.Add("TZA", "AF");
            countriesByContinents.Add("THA", "AS");
            countriesByContinents.Add("TGO", "AF");
            countriesByContinents.Add("TKL", "OC");
            countriesByContinents.Add("TON", "OC");
            countriesByContinents.Add("TTO", "NA");
            countriesByContinents.Add("TUN", "AF");
            countriesByContinents.Add("TUR", "AS");
            countriesByContinents.Add("TKM", "AS");
            countriesByContinents.Add("TCA", "NA");
            countriesByContinents.Add("TUV", "OC");
            countriesByContinents.Add("UMI", "OC");
            countriesByContinents.Add("VIR", "NA");
            countriesByContinents.Add("UGA", "AF");
            countriesByContinents.Add("UKR", "EU");
            countriesByContinents.Add("ARE", "AS");
            countriesByContinents.Add("GBR", "EU");
            countriesByContinents.Add("USA", "NA");
            countriesByContinents.Add("URY", "SA");
            countriesByContinents.Add("UZB", "AS");
            countriesByContinents.Add("VUT", "OC");
            countriesByContinents.Add("VAT", "EU");
            countriesByContinents.Add("VEN", "SA");
            countriesByContinents.Add("VNM", "AS");
            countriesByContinents.Add("WLF", "OC");
            countriesByContinents.Add("ESH", "AF");
            countriesByContinents.Add("YEM", "AS");
            countriesByContinents.Add("ZMB", "AF");
            countriesByContinents.Add("ZWE", "AF");
            countriesByContinents.Add("ALA", "EU");

            countriesByContinents.Add("AF", "AS");
            countriesByContinents.Add("AL", "EU");
            countriesByContinents.Add("DZ", "AF");
            countriesByContinents.Add("AS", "OC");
            countriesByContinents.Add("AD", "EU");
            countriesByContinents.Add("AO", "AF");
            countriesByContinents.Add("AI", "NA");
            countriesByContinents.Add("AQ", "AN");
            countriesByContinents.Add("AG", "NA");
            countriesByContinents.Add("AR", "SA");
            countriesByContinents.Add("AM", "AS");
            countriesByContinents.Add("AW", "NA");
            countriesByContinents.Add("AU", "OC");
            countriesByContinents.Add("AT", "EU");
            countriesByContinents.Add("AZ", "AS");
            countriesByContinents.Add("BS", "NA");
            countriesByContinents.Add("BH", "AS");
            countriesByContinents.Add("BD", "AS");
            countriesByContinents.Add("BB", "NA");
            countriesByContinents.Add("BY", "EU");
            countriesByContinents.Add("BE", "EU");
            countriesByContinents.Add("BZ", "NA");
            countriesByContinents.Add("BJ", "AF");
            countriesByContinents.Add("BM", "NA");
            countriesByContinents.Add("BT", "AS");
            countriesByContinents.Add("BO", "SA");
            countriesByContinents.Add("BQ", "NA");
            countriesByContinents.Add("BA", "EU");
            countriesByContinents.Add("BW", "AF");
            countriesByContinents.Add("BV", "AN");
            countriesByContinents.Add("BR", "SA");
            countriesByContinents.Add("IO", "AS");
            countriesByContinents.Add("VG", "NA");
            countriesByContinents.Add("BN", "AS");
            countriesByContinents.Add("BG", "EU");
            countriesByContinents.Add("BF", "AF");
            countriesByContinents.Add("BI", "AF");
            countriesByContinents.Add("KH", "AS");
            countriesByContinents.Add("CM", "AF");
            countriesByContinents.Add("CA", "NA");
            countriesByContinents.Add("CV", "AF");
            countriesByContinents.Add("KY", "NA");
            countriesByContinents.Add("CF", "AF");
            countriesByContinents.Add("TD", "AF");
            countriesByContinents.Add("CL", "SA");
            countriesByContinents.Add("CN", "AS");
            countriesByContinents.Add("CX", "OC");
            countriesByContinents.Add("CC", "AS");
            countriesByContinents.Add("CO", "SA");
            countriesByContinents.Add("KM", "AF");
            countriesByContinents.Add("CK", "OC");
            countriesByContinents.Add("CR", "NA");
            countriesByContinents.Add("HR", "EU");
            countriesByContinents.Add("CU", "NA");
            countriesByContinents.Add("CW", "NA");
            countriesByContinents.Add("CY", "EU");
            countriesByContinents.Add("CZ", "EU");
            countriesByContinents.Add("CD", "AF");
            countriesByContinents.Add("DK", "EU");
            countriesByContinents.Add("DJ", "AF");
            countriesByContinents.Add("DM", "NA");
            countriesByContinents.Add("DO", "NA");
            countriesByContinents.Add("TL", "OC");
            countriesByContinents.Add("EC", "SA");
            countriesByContinents.Add("EG", "AF");
            countriesByContinents.Add("SV", "NA");
            countriesByContinents.Add("GQ", "AF");
            countriesByContinents.Add("ER", "AF");
            countriesByContinents.Add("EE", "EU");
            countriesByContinents.Add("ET", "AF");
            countriesByContinents.Add("FK", "SA");
            countriesByContinents.Add("FO", "EU");
            countriesByContinents.Add("FJ", "OC");
            countriesByContinents.Add("FI", "EU");
            countriesByContinents.Add("FR", "EU");
            countriesByContinents.Add("GF", "SA");
            countriesByContinents.Add("PF", "OC");
            countriesByContinents.Add("TF", "AN");
            countriesByContinents.Add("GA", "AF");
            countriesByContinents.Add("GM", "AF");
            countriesByContinents.Add("GE", "AS");
            countriesByContinents.Add("DE", "EU");
            countriesByContinents.Add("GH", "AF");
            countriesByContinents.Add("GI", "EU");
            countriesByContinents.Add("GR", "EU");
            countriesByContinents.Add("GL", "NA");
            countriesByContinents.Add("GD", "NA");
            countriesByContinents.Add("GP", "NA");
            countriesByContinents.Add("GU", "OC");
            countriesByContinents.Add("GT", "NA");
            countriesByContinents.Add("GG", "EU");
            countriesByContinents.Add("GN", "AF");
            countriesByContinents.Add("GW", "AF");
            countriesByContinents.Add("GY", "SA");
            countriesByContinents.Add("HT", "NA");
            countriesByContinents.Add("HM", "AN");
            countriesByContinents.Add("HN", "NA");
            countriesByContinents.Add("HK", "AS");
            countriesByContinents.Add("HU", "EU");
            countriesByContinents.Add("IS", "EU");
            countriesByContinents.Add("IN", "AS");
            countriesByContinents.Add("ID", "AS");
            countriesByContinents.Add("IR", "AS");
            countriesByContinents.Add("IQ", "AS");
            countriesByContinents.Add("IE", "EU");
            countriesByContinents.Add("IM", "EU");
            countriesByContinents.Add("IL", "AS");
            countriesByContinents.Add("IT", "EU");
            countriesByContinents.Add("CI", "AF");
            countriesByContinents.Add("JM", "NA");
            countriesByContinents.Add("JP", "AS");
            countriesByContinents.Add("JE", "EU");
            countriesByContinents.Add("JO", "AS");
            countriesByContinents.Add("KZ", "AS");
            countriesByContinents.Add("KE", "AF");
            countriesByContinents.Add("KI", "OC");
            countriesByContinents.Add("XK", "EU");
            countriesByContinents.Add("KW", "AS");
            countriesByContinents.Add("KG", "AS");
            countriesByContinents.Add("LA", "AS");
            countriesByContinents.Add("LV", "EU");
            countriesByContinents.Add("LB", "AS");
            countriesByContinents.Add("LS", "AF");
            countriesByContinents.Add("LR", "AF");
            countriesByContinents.Add("LY", "AF");
            countriesByContinents.Add("LI", "EU");
            countriesByContinents.Add("LT", "EU");
            countriesByContinents.Add("LU", "EU");
            countriesByContinents.Add("MO", "AS");
            countriesByContinents.Add("MK", "EU");
            countriesByContinents.Add("MG", "AF");
            countriesByContinents.Add("MW", "AF");
            countriesByContinents.Add("MY", "AS");
            countriesByContinents.Add("MV", "AS");
            countriesByContinents.Add("ML", "AF");
            countriesByContinents.Add("MT", "EU");
            countriesByContinents.Add("MH", "OC");
            countriesByContinents.Add("MQ", "NA");
            countriesByContinents.Add("MR", "AF");
            countriesByContinents.Add("MU", "AF");
            countriesByContinents.Add("YT", "AF");
            countriesByContinents.Add("MX", "NA");
            countriesByContinents.Add("FM", "OC");
            countriesByContinents.Add("MD", "EU");
            countriesByContinents.Add("MC", "EU");
            countriesByContinents.Add("MN", "AS");
            countriesByContinents.Add("ME", "EU");
            countriesByContinents.Add("MS", "NA");
            countriesByContinents.Add("MA", "AF");
            countriesByContinents.Add("MZ", "AF");
            countriesByContinents.Add("MM", "AS");
            countriesByContinents.Add("NA", "AF");
            countriesByContinents.Add("NR", "OC");
            countriesByContinents.Add("NP", "AS");
            countriesByContinents.Add("NL", "EU");
            countriesByContinents.Add("AN", "NA");
            countriesByContinents.Add("NC", "OC");
            countriesByContinents.Add("NZ", "OC");
            countriesByContinents.Add("NI", "NA");
            countriesByContinents.Add("NE", "AF");
            countriesByContinents.Add("NG", "AF");
            countriesByContinents.Add("NU", "OC");
            countriesByContinents.Add("NF", "OC");
            countriesByContinents.Add("KP", "AS");
            countriesByContinents.Add("MP", "OC");
            countriesByContinents.Add("NO", "EU");
            countriesByContinents.Add("OM", "AS");
            countriesByContinents.Add("PK", "AS");
            countriesByContinents.Add("PW", "OC");
            countriesByContinents.Add("PS", "AS");
            countriesByContinents.Add("PA", "NA");
            countriesByContinents.Add("PG", "OC");
            countriesByContinents.Add("PY", "SA");
            countriesByContinents.Add("PE", "SA");
            countriesByContinents.Add("PH", "AS");
            countriesByContinents.Add("PN", "OC");
            countriesByContinents.Add("PL", "EU");
            countriesByContinents.Add("PT", "EU");
            countriesByContinents.Add("PR", "NA");
            countriesByContinents.Add("QA", "AS");
            countriesByContinents.Add("CG", "AF");
            countriesByContinents.Add("RO", "EU");
            countriesByContinents.Add("RU", "EU");
            countriesByContinents.Add("RW", "AF");
            countriesByContinents.Add("RE", "AF");
            countriesByContinents.Add("BL", "NA");
            countriesByContinents.Add("SH", "AF");
            countriesByContinents.Add("KN", "NA");
            countriesByContinents.Add("LC", "NA");
            countriesByContinents.Add("MF", "NA");
            countriesByContinents.Add("PM", "NA");
            countriesByContinents.Add("VC", "NA");
            countriesByContinents.Add("WS", "OC");
            countriesByContinents.Add("SM", "EU");
            countriesByContinents.Add("SA", "AS");
            countriesByContinents.Add("SN", "AF");
            countriesByContinents.Add("RS", "EU");
            countriesByContinents.Add("CS", "EU");
            countriesByContinents.Add("SC", "AF");
            countriesByContinents.Add("SL", "AF");
            countriesByContinents.Add("SG", "AS");
            countriesByContinents.Add("SX", "NA");
            countriesByContinents.Add("SK", "EU");
            countriesByContinents.Add("SI", "EU");
            countriesByContinents.Add("SB", "OC");
            countriesByContinents.Add("SO", "AF");
            countriesByContinents.Add("ZA", "AF");
            countriesByContinents.Add("GS", "AN");
            countriesByContinents.Add("KR", "AS");
            countriesByContinents.Add("SS", "AF");
            countriesByContinents.Add("ES", "EU");
            countriesByContinents.Add("LK", "AS");
            countriesByContinents.Add("SD", "AF");
            countriesByContinents.Add("SR", "SA");
            countriesByContinents.Add("SJ", "EU");
            countriesByContinents.Add("SZ", "AF");
            countriesByContinents.Add("SE", "EU");
            countriesByContinents.Add("CH", "EU");
            countriesByContinents.Add("SY", "AS");
            countriesByContinents.Add("ST", "AF");
            countriesByContinents.Add("TW", "AS");
            countriesByContinents.Add("TJ", "AS");
            countriesByContinents.Add("TZ", "AF");
            countriesByContinents.Add("TH", "AS");
            countriesByContinents.Add("TG", "AF");
            countriesByContinents.Add("TK", "OC");
            countriesByContinents.Add("TO", "OC");
            countriesByContinents.Add("TT", "NA");
            countriesByContinents.Add("TN", "AF");
            countriesByContinents.Add("TR", "AS");
            countriesByContinents.Add("TM", "AS");
            countriesByContinents.Add("TC", "NA");
            countriesByContinents.Add("TV", "OC");
            countriesByContinents.Add("UM", "OC");
            countriesByContinents.Add("VI", "NA");
            countriesByContinents.Add("UG", "AF");
            countriesByContinents.Add("UA", "EU");
            countriesByContinents.Add("AE", "AS");
            countriesByContinents.Add("GB", "EU");
            countriesByContinents.Add("US", "NA");
            countriesByContinents.Add("UY", "SA");
            countriesByContinents.Add("UZ", "AS");
            countriesByContinents.Add("VU", "OC");
            countriesByContinents.Add("VA", "EU");
            countriesByContinents.Add("VE", "SA");
            countriesByContinents.Add("VN", "AS");
            countriesByContinents.Add("WF", "OC");
            countriesByContinents.Add("EH", "AF");
            countriesByContinents.Add("YE", "AS");
            countriesByContinents.Add("ZM", "AF");
            countriesByContinents.Add("ZW", "AF");
            countriesByContinents.Add("AX", "EU");
        }

        public void SortByGeolocation(List<Service> discoverServices, List<Service> returnedServices, string clientAddr)
        {
            //clientAddr = "4.15.18.9";
            Dictionary<Service, double> dictClosest = new Dictionary<Service, double>();
            if (!_memoryCache.TryGetValue("_geo_" + clientAddr, out GeoLocation clientLocation) || (clientLocation == null))
            {
                clientLocation = _geoServiceProvider.GeoLocate(clientAddr);
                _memoryCache.Set("_geo_" + clientAddr, clientLocation, TimeSpan.FromHours(24));
            }

            // Geolocate the services
            foreach (var service in discoverServices)
            {
                if (!_memoryCache.TryGetValue("_geo_" + service.Address, out GeoLocation serviceLocation) || (serviceLocation == null))
                {
                    serviceLocation = _geoServiceProvider.GeoLocate(service.Address);
                    _memoryCache.Set("_geo_" + service.Address, serviceLocation, TimeSpan.FromHours(24));
                }

                if (!string.IsNullOrEmpty(service.Geo.Continents) && (!string.IsNullOrEmpty(clientLocation.CountryCode)))
                {
                    string[] continents = service.Geo.Continents.Split(',');
                    string continent = (string)countriesByContinents[clientLocation.CountryCode];
                    if (continents.Contains(continent))
                        returnedServices.Add(service);
                }
                if (!string.IsNullOrEmpty(service.Geo.Countries) && (!string.IsNullOrEmpty(clientLocation.CountryCode)))
                {
                    string[] countries = service.Geo.Countries.Split(',');
                    if (countries.Contains(clientLocation.CountryCode))
                        returnedServices.Add(service);
                }
                if (!string.IsNullOrEmpty(service.Geo.Regions) && (!string.IsNullOrEmpty(clientLocation.RegionCode)))
                {
                    string[] states = service.Geo.Regions.Split(',');
                    if (states.Contains(clientLocation.RegionCode))
                        returnedServices.Add(service);
                }
                if (!string.IsNullOrEmpty(service.Geo.GeoPos) && (!string.IsNullOrEmpty(clientLocation.Lat)))
                {
                    string[] geopos = service.Geo.GeoPos.Split(',');
                    if (geopos.Length == 2)
                    {
                        double lat = double.Parse(geopos[0]);
                        double lon = double.Parse(geopos[1]);
                        var distance = new Coordinates(lat, lon)
                                        .DistanceTo(
                                            new Coordinates(double.Parse(clientLocation.Lat), double.Parse(clientLocation.Lon)),
                                            UnitOfLength.Kilometers
                                        );

                        dictClosest.Add(service, distance);                       
                    }
                    else
                    if ((geopos.Length == 1) && (geopos[0] == "client") && !string.IsNullOrEmpty(clientLocation.Lat) && !string.IsNullOrEmpty(clientLocation.Lon))
                    {
                        double lat = double.Parse(clientLocation.Lat);
                        double lon = double.Parse(clientLocation.Lon);
                        var distance = new Coordinates(lat, lon)
                                        .DistanceTo(
                                            new Coordinates(double.Parse(serviceLocation.Lat), double.Parse(serviceLocation.Lon)),
                                            UnitOfLength.Kilometers
                                        );

                        dictClosest.Add(service, distance);
                    }
                }
            }
            if (dictClosest.Count > 0)
            {
                // Perform Random for GeoEquidistant nodes.
                var servicesOrdered = dictClosest.OrderBy(e => e.Value).ToList();
                var groupedbyOrder = servicesOrdered.GroupBy(g => g.Value).ToList();
                
                foreach(var g in groupedbyOrder)
                {
                    List<Service> returnedByGroup = new List<Service>();
                    Random rnd = new Random(DateTime.UtcNow.Millisecond);
                    var group = g.Select(s=>s.Key).ToList();
                    while (group.Count != returnedByGroup.Count)
                    {
                        var next = rnd.Next(group.Count);
                        if (returnedByGroup.Contains(group[next])) continue;
                        group[next].LastChoosed = false;
                        if (returnedServices.Count == 0) group[next].LastChoosed = true;
                        returnedByGroup.Add(group[next]);
                    }
                    returnedServices.AddRange(returnedByGroup);
                }
            }
        }
    }
}
