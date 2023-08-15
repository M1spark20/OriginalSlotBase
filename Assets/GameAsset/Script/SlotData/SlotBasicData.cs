using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class SlotBasicData : SlotMaker2022.ILocalDataInterface
{
	// 定数
	public const byte CREDIT_MAX = 50;

	public byte slotSetting { get; private set; }
	
	public uint inCount { get; private set; }
	public uint outCount { get; private set; }
	
	public byte betCount { get; private set; }
	public byte creditShow { get; private set; }
	public byte payoutShow { get; private set; }
	public bool isReplay { get; private set; }
	
	public byte gameMode { get; private set; }
	public byte RTMode { get; private set; }
	
	public byte bonusFlag { get; private set; }
	public byte castFlag { get; private set; }
	
	public SlotBasicData(){
		slotSetting	=  5;
		inCount		=  0;
		outCount	=  0;
		betCount	=  0;
		creditShow	= 50;
		payoutShow	=  0;
		isReplay	= false;
		gameMode	=  0;
		RTMode		=  0;
		bonusFlag	=  0;
		castFlag	=  0;
	}
	
	public bool StoreData(ref BinaryWriter fs, int version){
		fs.Write(inCount);
		fs.Write(outCount);
		fs.Write(betCount);
		fs.Write(creditShow);
		fs.Write(payoutShow);
		fs.Write(isReplay);
		fs.Write(gameMode);
		fs.Write(RTMode);
		fs.Write(bonusFlag);
		fs.Write(castFlag);
		return true;
	}
	public bool ReadData(ref BinaryReader fs, int version){
		inCount		= fs.ReadUInt32();
		outCount	= fs.ReadUInt32();
		betCount	= fs.ReadByte();
		creditShow	= fs.ReadByte();
		payoutShow	= fs.ReadByte();
		isReplay	= fs.ReadBoolean();
		gameMode	= fs.ReadByte();
		RTMode		= fs.ReadByte();
		bonusFlag	= fs.ReadByte();
		castFlag	= fs.ReadByte();
		return true;
	}
	
	// 変数設定用メソッド
	public void AddBetCount(){
		++betCount;
		--creditShow;
		payoutShow = 0;
	}
	public void ClearBetCount(){
		creditShow = (byte)Math.Max(creditShow + betCount, CREDIT_MAX);
		betCount = 0;
	}
}
