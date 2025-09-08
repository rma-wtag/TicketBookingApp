CREATE PROCEDURE GetAvailableSeats
    @ShowId INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @HallId INT;

    SELECT @HallId = HallId
    FROM Shows
    WHERE Id = @ShowId;

    IF @HallId IS NULL
    BEGIN
        RETURN;
    END

    SELECT s.*
    FROM Seats s
    WHERE s.HallId = @HallId
      AND NOT EXISTS (
          SELECT 1
          FROM BookingSeats bs
          INNER JOIN Bookings b ON bs.BookingId = b.Id
          WHERE bs.SeatId = s.Id
            AND b.ShowId = @ShowId
      );
END