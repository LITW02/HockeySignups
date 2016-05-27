using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;

namespace HockeySignups.Data
{
    public class HockeySignupsDb
    {
        private readonly string _connectionString;

        public HockeySignupsDb(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void AddEvent(Event e)
        {
            InitiateDbAction(cmd =>
            {
                cmd.CommandText = "INSERT INTO Events (Date, MaxPeople) VALUES (@Date, @MaxPeople); SELECT @@Identity";
                cmd.Parameters.AddWithValue("@Date", e.Date);
                cmd.Parameters.AddWithValue("@MaxPeople", e.MaxPeople);

                e.Id = (int)(decimal)cmd.ExecuteScalar();
            });
        }

        public Event GetEventById(int eventId)
        {
            Event e = null;
            InitiateDbAction(cmd =>
            {
                cmd.CommandText = "SELECT * FROM Events WHERE Id = @id";
                cmd.Parameters.AddWithValue("@id", eventId);
                var reader = cmd.ExecuteReader();
                reader.Read();
                e = new Event
                     {
                         Date = (DateTime)reader["Date"],
                         Id = (int)reader["Id"],
                         MaxPeople = (int)reader["MaxPeople"]
                     };
            });

            return e;
        }

        public Event GetLatestEvent()
        {
            Event e = null;
            InitiateDbAction(cmd =>
            {
                cmd.CommandText = "SELECT TOP 1 * FROM Events ORDER BY Id DESC";
                var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    e = new Event
                    {
                        Date = (DateTime)reader["Date"],
                        Id = (int)reader["Id"],
                        MaxPeople = (int)reader["MaxPeople"]
                    };
                }
            });

            return e;
        }

        public IEnumerable<Event> GetEvents()
        {
            List<Event> events = new List<Event>();
            InitiateDbAction(cmd =>
            {
                cmd.CommandText = "SELECT * FROM Events ORDER BY Date DESC";
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var e = new Event
                    {
                        Date = (DateTime)reader["Date"],
                        Id = (int)reader["Id"],
                        MaxPeople = (int)reader["MaxPeople"]
                    };
                    events.Add(e);
                }
            });

            return events;
        }

        public IEnumerable<EventSignup> GetEventSignups(int eventId)
        {
            var result = new List<EventSignup>();

            InitiateDbAction(cmd =>
            {
                cmd.CommandText = "SELECT * FROM EventSignups WHERE EventId = @eventId";
                cmd.Parameters.AddWithValue("@eventId", eventId);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    EventSignup es = new EventSignup
                    {
                        Id = (int)reader["Id"],
                        Email = (string)reader["Email"],
                        EventId = (int)reader["EventId"],
                        FirstName = (string)reader["FirstName"],
                        LastName = (string)reader["LastName"]
                    };
                    result.Add(es);
                }
            });

            return result;
        }

        public void AddEventSignup(EventSignup es)
        {
            InitiateDbAction(cmd =>
            {
                cmd.CommandText =
                    "INSERT INTO EventSignups (Email, FirstName, LastName, EventId) VALUES (@email, @firstName, @lastName, @eventId)";
                cmd.Parameters.AddWithValue("@email", es.Email);
                cmd.Parameters.AddWithValue("@firstName", es.FirstName);
                cmd.Parameters.AddWithValue("@lastName", es.LastName);
                cmd.Parameters.AddWithValue("@eventId", es.EventId);
                cmd.ExecuteNonQuery();
            });
        }

        public IEnumerable<NotificationSignup> GetNotificationSignups()
        {
            var result = new List<NotificationSignup>();

            InitiateDbAction(cmd =>
            {
                cmd.CommandText = "SELECT * FORM NotificationSignups";
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    NotificationSignup ns = new NotificationSignup
                    {
                        Id = (int)reader["Id"],
                        Email = (string)reader["Email"],
                        FirstName = (string)reader["FirstName"],
                        LastName = (string)reader["LastName"]
                    };
                    result.Add(ns);
                }
            });

            return result;
        }

        public EventStatus GetEventStatus(Event e)
        {
            if (e == null || e.Date < DateTime.Today)
            {
                return EventStatus.InThePast;
            }

            int pplAmount = 0;
            InitiateDbAction(cmd =>
            {
                cmd.CommandText = "SELECT COUNT(*) FROM EventSignups WHERE EventId = @eventId";
                cmd.Parameters.AddWithValue("@eventId", e.Id);
                pplAmount = (int)cmd.ExecuteScalar();
            });

            if (pplAmount < e.MaxPeople)
            {
                return EventStatus.Open;
            }

            return EventStatus.Full;
        }

        public void AddNotificationSignup(NotificationSignup ns)
        {
            InitiateDbAction(cmd =>
            {
                cmd.CommandText =
                    "INSERT INTO NotificationSignups (Email, FirstName, LastName) VALUES (@email, @firstName, @lastName)";
                cmd.Parameters.AddWithValue("@email", ns.Email);
                cmd.Parameters.AddWithValue("@firstName", ns.FirstName);
                cmd.Parameters.AddWithValue("@lastName", ns.LastName);
                cmd.ExecuteNonQuery();
            });
        }

        public IEnumerable<EventWithPeople> GetEventsWithCount()
        {
            var result = new List<EventWithPeople>();
            InitiateDbAction(cmd =>
            {
                cmd.CommandText = "GetEventsWithCount";
                cmd.CommandType = CommandType.StoredProcedure;
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    result.Add(new EventWithPeople
                    {
                        Date = (DateTime)reader["Date"],
                        MaxPeople = (int)reader["MaxPeople"],
                        PeopleCount = (int)reader["PeopleCount"],
                        Id = (int)reader["Id"]
                    });
                }
            });

            return result;
        }

        private void InitiateDbAction(Action<SqlCommand> action)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand cmd = connection.CreateCommand())
            {
                connection.Open();
                action(cmd);
            }
        }
    }
}