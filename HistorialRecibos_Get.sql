ALTER PROCEDURE dbo.HistorialRecibos_Get
AS
BEGIN
	
	SELECT '00' RESPCODE, 'OK' DESCCODE
	
	SELECT top 10 ReferenciaNumericaXPayId ID, 'IMSS' Servicio, ReferenciaNumerica ReferenciaNumerica, 1109.00 IMPORTE, CONVERT(VARCHAR,FechaExpiracion,103) Vigencia, 'Pagado' Estatus
	FROM ReferenciaNumericaXPay
	order by FechaCreacion desc 
END