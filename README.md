# DogecoinTerminal

DogecoinTerminal is a multi-user **offline** dogecoin wallet made using *Monogame* and *[libdogecoin](https://github.com/dogecoinfoundation/libdogecoin)* for maximum portability.

## What you'll need:

DogecoinTerminal is designed to run on `An Always Offline Device with a Camera`, so you'll need:

* An Always Offline Device with a Camera
* An Android Device with a Camera. *(for QRDoge)*
* An [Dogecoin](https://github.com/dogecoin/dogecoin) node *(for QRDoge)*


![image](https://github.com/UsaRandom/DogecoinTerminal/assets/2897796/f0fb780f-2960-4a5c-8e46-4cf1ec3e675e)

![image](https://github.com/UsaRandom/DogecoinTerminal/assets/2897796/f2ff4988-407c-489e-a37b-02f91d50a2ed)



# Important Notes

* **DogecoinTerminal cannot send transactions out of the box** (read more on 'Bridging to the Network')
* **Loading old keys not currently supported!** You *can* load them by hand, but getting UTXOs (balance) could also become annoying.


## Security
--------

Security comes from the device being offline, your ability to keep it offline, and your ability to keep it physically secure.

Currently, wallet data (mnemonics/utxos) is encrypted when stored but it would be trivial to decrypt for a motiviated actor.

Different network bridges might provide different levels of security.

QRDoge isn't pretty, but you can observe the data before consuming it, which is useful in it's own right.



## Settings
----

* `fee-per-utxo` (default: 0.02): Used to calculate network fees, `fee-per-utxo` * `count(UTXOsUsedInTransaction)` = `networkFee`
* `dust-limit`: (default: 0.001): The dust limit used in transaction creation.



## Bridging to the Network

**DogecoinTerminal cannot send transactions on it's own**, it can only sign them.

To send transactions, you'll need a bridge. (an implimentation of `IDogecoinService`)

Currently there is one bridge, [DogecoinTerminal.QRDoge.QRDogeService](https://github.com/UsaRandom/DogecoinTerminal/blob/master/DogecoinTerminal.QRDoge/QRDogecoinService.cs).

It uses QR codes to pass messages back and forth with it's companion app:

### [QRDoge](https://github.com/UsaRandom/QRDoge)

A rudamentary android companion app that acts as a bridge between your [Dogecoin Core Node](https://github.com/dogecoin/dogecoin) and DogecoinTerminal.


![image](https://github.com/UsaRandom/DogecoinTerminal/assets/2897796/876af895-1897-46d0-be58-1e05c223e231)



## Creating your own Bridge

To create your own bridge, create a new project with a reference to `DogecoinTerminal.Common` and impliment the `IDogecoinService` interface.

```csharp
	//NOTE: not a final interface, only an example.
	public interface IDogecoinService
	{
		void OnSetup(Action<bool> callback);

		void OnNewAddress(string address, string pin, Action<bool> callback);

		void OnDeleteAddress(string address, string pin, Action callback);

		void UpdatePin(string address, string oldPin, string newPin, Action<bool> callback);

		//These are the only two functions that are required to send/receive transactions.
		//pin string param is likely going away, nothing depends on it currently.
		void GetUTXOs(string address, string pin, Action<IEnumerable<UTXOInfo>> callback);

		void SendTransaction(string transaction, string pin, Action<bool> callback);
	}
```


The `DogecoinTerminal.Common.QRDoge.QRDogecoinService` class is a good reference on creating a service.


