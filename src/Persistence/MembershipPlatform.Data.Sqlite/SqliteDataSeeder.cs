using Microsoft.Data.Sqlite;

namespace MembershipPlatform.Data.Sqlite;

public static class SqliteDataSeeder
{
    public static async Task SeedAsync(
        string connectionString,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT OR IGNORE INTO Members
                (MemberId, Name, Email, Status, JoinDate)
            VALUES
                ('11111111-1111-1111-1111-111111111111',
                 'Active Member',
                 'active@example.com',
                 0,
                 '2026-08-01T09:00:00.0000000+00:00'),
                ('22222222-2222-2222-2222-222222222222',
                 'Inactive Member',
                 'inactive@example.com',
                 1,
                 '2026-07-01T09:00:00.0000000+00:00'),
                ('33333333-3333-3333-3333-333333333333',
                 'Maya Chen',
                 'maya.chen@example.com',
                 0,
                 '2026-04-12T09:00:00.0000000+00:00'),
                ('44444444-4444-4444-4444-444444444444',
                 'Jordan Brooks',
                 'jordan.brooks@example.com',
                 0,
                 '2026-05-03T09:00:00.0000000+00:00'),
                ('55555555-5555-5555-5555-555555555555',
                 'Priya Shah',
                 'priya.shah@example.com',
                 0,
                 '2026-05-18T09:00:00.0000000+00:00'),
                ('66666666-6666-6666-6666-666666666666',
                 'Lucas Martin',
                 'lucas.martin@example.com',
                 0,
                 '2026-06-07T09:00:00.0000000+00:00'),
                ('77777777-7777-7777-7777-777777777777',
                 'Sofia Reyes',
                 'sofia.reyes@example.com',
                 1,
                 '2026-03-21T09:00:00.0000000+00:00'),
                ('88888888-8888-8888-8888-888888888888',
                 'Ethan Walker',
                 'ethan.walker@example.com',
                 0,
                 '2026-07-14T09:00:00.0000000+00:00');

            INSERT OR IGNORE INTO Classes
                (ClassId, Name, StartTime, Capacity)
            VALUES
                ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
                 'Morning Yoga',
                 '2026-08-20T09:00:00.0000000+00:00',
                 20),
                ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb',
                 'Strength Training',
                 '2026-08-21T17:30:00.0000000+00:00',
                 12),
                ('c1c1c1c1-c1c1-c1c1-c1c1-c1c1c1c1c1c1',
                 'Pilates Fundamentals',
                 '2026-08-22T10:30:00.0000000+00:00',
                 16),
                ('c2c2c2c2-c2c2-c2c2-c2c2-c2c2c2c2c2c2',
                 'Evening Spin',
                 '2026-08-22T18:00:00.0000000+00:00',
                 10),
                ('c3c3c3c3-c3c3-c3c3-c3c3-c3c3c3c3c3c3',
                 'Functional Mobility',
                 '2026-08-23T08:30:00.0000000+00:00',
                 18),
                ('c4c4c4c4-c4c4-c4c4-c4c4-c4c4c4c4c4c4',
                 'Weekend Boxing',
                 '2026-08-23T11:00:00.0000000+00:00',
                 8),
                ('c5c5c5c5-c5c5-c5c5-c5c5-c5c5c5c5c5c5',
                 'Core Conditioning',
                 '2026-08-24T17:00:00.0000000+00:00',
                 14),
                ('c6c6c6c6-c6c6-c6c6-c6c6-c6c6c6c6c6c6',
                 'Recovery Stretch',
                 '2026-08-25T12:00:00.0000000+00:00',
                 10);

            INSERT OR IGNORE INTO CheckIns
                (CheckInId, MemberId, CheckInDate)
            VALUES
                ('cccccccc-cccc-cccc-cccc-cccccccccccc',
                 '11111111-1111-1111-1111-111111111111',
                 '2026-08-17T09:00:00.0000000+00:00'),
                ('e0000000-0000-0000-0000-000000000001',
                 '33333333-3333-3333-3333-333333333333',
                 '2026-08-15T08:42:00.0000000+00:00'),
                ('e0000000-0000-0000-0000-000000000002',
                 '33333333-3333-3333-3333-333333333333',
                 '2026-08-17T08:55:00.0000000+00:00'),
                ('e0000000-0000-0000-0000-000000000003',
                 '44444444-4444-4444-4444-444444444444',
                 '2026-08-16T17:21:00.0000000+00:00'),
                ('e0000000-0000-0000-0000-000000000004',
                 '55555555-5555-5555-5555-555555555555',
                 '2026-08-16T09:12:00.0000000+00:00'),
                ('e0000000-0000-0000-0000-000000000005',
                 '55555555-5555-5555-5555-555555555555',
                 '2026-08-18T09:05:00.0000000+00:00'),
                ('e0000000-0000-0000-0000-000000000006',
                 '66666666-6666-6666-6666-666666666666',
                 '2026-08-17T18:02:00.0000000+00:00'),
                ('e0000000-0000-0000-0000-000000000007',
                 '88888888-8888-8888-8888-888888888888',
                 '2026-08-18T07:48:00.0000000+00:00');

            INSERT OR IGNORE INTO ClassRegistrations
                (RegistrationId, ClassId, MemberId, RegisteredDate, Status)
            VALUES
                ('dddddddd-dddd-dddd-dddd-dddddddddddd',
                 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb',
                 '11111111-1111-1111-1111-111111111111',
                 '2026-08-17T10:00:00.0000000+00:00',
                 0),
                ('d0000000-0000-0000-0000-000000000001',
                 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
                 '33333333-3333-3333-3333-333333333333',
                 '2026-08-12T10:00:00.0000000+00:00',
                 0),
                ('d0000000-0000-0000-0000-000000000002',
                 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
                 '55555555-5555-5555-5555-555555555555',
                 '2026-08-13T10:00:00.0000000+00:00',
                 0),
                ('d0000000-0000-0000-0000-000000000003',
                 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb',
                 '44444444-4444-4444-4444-444444444444',
                 '2026-08-13T11:00:00.0000000+00:00',
                 0),
                ('d0000000-0000-0000-0000-000000000004',
                 'c1c1c1c1-c1c1-c1c1-c1c1-c1c1c1c1c1c1',
                 '33333333-3333-3333-3333-333333333333',
                 '2026-08-14T09:00:00.0000000+00:00',
                 0),
                ('d0000000-0000-0000-0000-000000000005',
                 'c1c1c1c1-c1c1-c1c1-c1c1-c1c1c1c1c1c1',
                 '66666666-6666-6666-6666-666666666666',
                 '2026-08-14T09:05:00.0000000+00:00',
                 0),
                ('d0000000-0000-0000-0000-000000000006',
                 'c2c2c2c2-c2c2-c2c2-c2c2-c2c2c2c2c2c2',
                 '44444444-4444-4444-4444-444444444444',
                 '2026-08-15T12:00:00.0000000+00:00',
                 0),
                ('d0000000-0000-0000-0000-000000000007',
                 'c2c2c2c2-c2c2-c2c2-c2c2-c2c2c2c2c2c2',
                 '88888888-8888-8888-8888-888888888888',
                 '2026-08-15T12:05:00.0000000+00:00',
                 0),
                ('d0000000-0000-0000-0000-000000000008',
                 'c3c3c3c3-c3c3-c3c3-c3c3-c3c3c3c3c3c3',
                 '55555555-5555-5555-5555-555555555555',
                 '2026-08-16T08:00:00.0000000+00:00',
                 0),
                ('d0000000-0000-0000-0000-000000000009',
                 'c3c3c3c3-c3c3-c3c3-c3c3-c3c3c3c3c3c3',
                 '66666666-6666-6666-6666-666666666666',
                 '2026-08-16T08:05:00.0000000+00:00',
                 0),
                ('d0000000-0000-0000-0000-000000000010',
                 'c4c4c4c4-c4c4-c4c4-c4c4-c4c4c4c4c4c4',
                 '33333333-3333-3333-3333-333333333333',
                 '2026-08-16T09:00:00.0000000+00:00',
                 0),
                ('d0000000-0000-0000-0000-000000000011',
                 'c5c5c5c5-c5c5-c5c5-c5c5-c5c5c5c5c5c5',
                 '88888888-8888-8888-8888-888888888888',
                 '2026-08-17T09:00:00.0000000+00:00',
                 0);
            """;

        await using var connection = new SqliteConnection(
            SqliteConnectionSettings.Normalize(connectionString));
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
