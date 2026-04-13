ALTER PROCEDURE HistorialRecibosDetalle_Get
(
	@ReferenciaNumericaId INT 
)
AS
BEGIN
	SELECT '00' RESPCODE, 'OK' DESCCODE
	
	SELECT top 10 ReferenciaNumericaXPayId ID, 'IMSS' Servicio, ReferenciaNumerica ReferenciaNumerica, 1109.00 IMPORTE, CONVERT(VARCHAR,FechaExpiracion,103) Vigencia, 'Pagado' Estatus
	FROM ReferenciaNumericaXPay
	WHERE ReferenciaNumericaXPayId = @ReferenciaNumericaId
	order by FechaCreacion desc 
END