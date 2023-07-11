using NssIT.Kiosk.AppDecorator.DomainLibs.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Train.Kiosk.Security.Live.BTnG
{
    public class BTnGLiveGuard : IBTnGGuardInfo
    {
        private static BTnGLiveGuard _guard = new BTnGLiveGuard();

        public static IBTnGGuardInfo GuardInfo
        {
            get
            {
                return _guard;
            }
        }

        public string MerchantId { get; } = "KTMB-Kiosk";

        public string WebApiUrl { get; } = @"https://ktmb-live-api.azurewebsites.net/api";

        private BTnGLiveGuard() { }

        public string PublicKey
        {
            get
            {
                return @"-----BEGIN PUBLIC KEY-----
MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAmlnJoTzXqqYTWiv2DAq3
j7jQe7fGaOh8gJEK7fBL8sf2a5Cpx+to6aGTassZgPKfd2ECUr9KK3EdYxeUQhZW
QSEeDlY8hlQ+LIHz83t8SaHDjhSsQIaDOXLCNBQ4ZUiRGTS94tpw2jux995ZErzq
zfvK8XvXbg7V4FLgprWU/ctqB0yqZpG8JxNcCn2wFH4HrVNcPwpVyFWcfEvuIEdH
VpnLL/hBcrZMEnt+fUSkR/q7p04YZrYy0ubGKbyK9JD6nMernPfLY5/01NWVrJa9
EmsSgHzjTsff2aGkktTpriEfxBpQbqySVoxzQQwCnNQmfcPixqO26jO62c4ApmJP
DQIDAQAB
-----END PUBLIC KEY-----";
            }
        }

        public string PrivateKey
        {
            get
            {
                return @"-----BEGIN RSA PRIVATE KEY-----
MIIEoQIBAAKCAQB314CzcIqiGKAn3svVS1qb3v4kgn1869ODFeNZdNLFPeM+SVbz
xAm1Rr6BacdW7pOPDYm/Jghv6Tu2ZOm9rKyISx9X1yd1ulEdjS6ZEDDZMkiiIAmw
LNkAj0Gi464GRAsp/tYCZbg0MczgXzzGAdSQN5SLvseHlN4jyrzzOJ1O+gr15NmQ
9SRJUwvGuyBUugJ95KM4G4iIIxEj6ZwUUxVcJ8GKCTAEzBbgWoIckQJ+fl/wLWuw
FConbBY5Ofnyp2sEUNDRXQH41GxpIbmMnltRuxXHgY5ZKykXo4YPlfyvfs3ZWfHy
2aMGM0iHpk+wJTaBmPk0MRRh9B2t4j4j7g61AgMBAAECggEAWpDSo0iejAdeKSNX
GTbcH8VyfDbdDKXPb1YAPR9rFUJC1FRqUMfKqmLvHy/IRGsBum75tSnR4Hj69kjm
17+Bl3JmGmTvXinSmSmTmHrC37D/cP4TND+AMssQSEAVyIhtg2whYShr/43PDZUL
gXvqnp5SNRWVBmI7SgMxrgEP6JlGrl3+svGEI8IlUsd7/38gVqeJTWPQLX2Mm7CU
KRyFE3PL0tuoKxmwg7iVcNFjFeWRZveQArRKzlVxJb4TpYJi6GFsralYGyUfFPIm
GPzgXdUi5o3txbSDv6CCULlxmKA8I6NJNry5T6t+epTz1eR7nr2VYecsn+UN60nr
+VP24QKBgQC5+zdRY+ANiyvG08SxSNe6rDQf4IHSJrTE1h1vRUfOkJ/ozmLPZRNI
5rm0bWATADYYSWcxsyLpBpu7VxaPrey+V4Q3fVxQHp/iHsL0xMbY6z1r4RMP2KPW
7oQQ18pWIXkcta88/V2FOBw9EFGdQXRuspE7aLF0cKGS/E5sikEZXQKBgQCk9cuA
OLNxWNN13NyF8y0KBqvdzwGtPDirlLllrwo19cgpaobY1pXWx+HVIB9DeI1Egtxq
rX1ddfmxgLa7WuilABl1y/O41UCTz69p4TNZOKPg81LgApJIJdLkLBwg8Y4u3kWt
Jgqx7usvmKmv4TdW+xll0AYLXhU2uR/04HF9OQKBgFCLow6FwcA72M64YkQhdwSq
lbyEf9Ti3IEu9fp9t7aXXhn2YEbP0IVPH3grsmMoQUVNbPrSSTlAmmuNMQEy4wgg
YOujSDqOuNG2Xtqg6jmBz80L4jSHr5VjISEmQ8P/pTMw4F76n+kVPu2XRrFjaqiS
f2GKWuhB60f8K37IdZ8lAoGAXvfq+pyqIh1DUF2gz7UaPuVsWLxueK9s6MssmS/W
rw4a+E0N6RRRG1/j0fE2Tn++xKyZ/Qh4XjAnWluB2AcUXLnb7odGfyi2juJIWKqA
B2OmZUFweurAEK1F5nrGjujCNJVeg7qc+adU2wgG2OzOEVNqYcULVcwKZoDZtv7o
F3ECgYBgVR4TAu1eYjepXhlitPm14d2Rc6FBMpt3919TgU1Wnbo+fIDWjBS1k0OF
wjbfApo/MoXHJNgRFFDG0Y3i6KDf/zIgaatySNawYax/Iy/+MW4Ft8K15uQ7I9Kx
7Vyq5/I8slCalXLTHwhrrIRUnD2nHLleIUcIRvTE6xDgh/3zQg==
-----END RSA PRIVATE KEY-----";
            }
        }
    }
}
