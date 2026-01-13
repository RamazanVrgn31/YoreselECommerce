// Ödeme Bilgileri Modeli
export interface PaymentDto {
    cardHolderName: string;
    cardNumber: string;
    expireMonth: string;
    expireYear: string;
    cvv: string;
    price: number;
}

// Sipariş Oluşturma Modeli (Backend'e gidecek ana paket)
export interface OrderCreateDto {
    address: string;
    PaymentDto: PaymentDto;
}

//Backend'den dönen sipariş sonucu modeli
export interface OrderDto {
    orderId: number;
    customerId: number;
    customerName: string;
    orderDate: string;  // C# DateTime -> TS string
    totalAmount: number;    // C# decimal -> TS number
    status: string;
    address: string;
}

export interface OrderDetailDto {
    productName: string;
    quantity: number;
    price: number;  // C# decimal -> TS number
    total: number;  // C# decimal -> TS number
}