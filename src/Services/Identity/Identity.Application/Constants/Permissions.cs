namespace Identity.Application.Constants
{
    public static class Permissions
    {
        public static class Users
        {
            public const string Read = "users.read";
            public const string Write = "users.write";
            public const string Delete = "users.delete";
        }

        public static class Patients
        {
            public const string Read = "patients.read";
            public const string Write = "patients.write";
            public const string Delete = "patients.delete";
        }

        public static class Appointments
        {
            public const string Read = "appointments.read";
            public const string Write = "appointments.write";
            public const string Delete = "appointments.delete";
        }

        public static class Billing
        {
            public const string Read = "billing.read";
            public const string Write = "billing.write";
            public const string Delete = "billing.delete";
        }

        public static class Treatments
        {
            public const string Read = "treatments.read";
            public const string Write = "treatments.write";
            public const string Delete = "treatments.delete";
        }
    }
}
